using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Model.Admin;
using Tellma.Repository.Admin;
using Tellma.Repository.Common;
using Tellma.Utilities.Common;

namespace Tellma.ApplicationDbPublisher
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            #region Cancellation Token

            // Setup the cancellation token
            CancellationToken cancellation;
            {
                var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (s, e) =>
                {
                    if (!cts.Token.IsCancellationRequested)
                    {
                        WriteLine();
                        WriteLine("Canceling...", ConsoleColor.DarkYellow);
                        cts.Cancel();
                        e.Cancel = true;
                    }
                };

                cancellation = cts.Token;
            }

            Write("Press ");
            Write("Ctrl+C", ConsoleColor.DarkYellow);
            Write(" to cancel anytime...");
            WriteLine();
            WriteLine();

            #endregion

            try
            {
                await PublishDatabases(args, cancellation);
            }
            catch (OperationCanceledException)
            {
                WriteLine("Publish aborted.", ConsoleColor.DarkYellow);
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message, ConsoleColor.Red);
            }

            if (!cancellation.IsCancellationRequested)
            {
                WriteLine();
                Write("Press Enter to exit...");
                ReadLine();
            }
        }

        private static async Task PublishDatabases(string[] args, CancellationToken cancellation)
        {
            #region Arguments

            PublisherOptions opt;
            SqlConnectionStringBuilder adminConnBldr = null;
            {
                // Build the configuration root based on project user secrets
                IConfiguration config = new ConfigurationBuilder()
                    .AddUserSecrets(typeof(Program).Assembly)
                    .AddCommandLine(args) // Higher precedence
                    .Build();

                opt = config.Get<PublisherOptions>();

                // DACPAC File (Required)
                if (!opt.SkipPublish)
                {
                    while (string.IsNullOrWhiteSpace(opt.DacpacFile))
                    {
                        Write("Enter path to DACPAC file: ");
                        opt.DacpacFile = ReadLine();

                        if (opt.DacpacFile == null)
                        {
                            throw new OperationCanceledException();
                        }
                    }

                    if (!opt.DacpacFile.EndsWith(".dacpac"))
                    {
                        throw new ArgumentException($"DACPAC file must have the \".dacpac\" extension");
                    }
                    else if (!File.Exists(opt.DacpacFile))
                    {
                        throw new ArgumentException($"No DACPAC found at \"{opt.DacpacFile}\"");
                    }
                }

                // Admin Connection (Required)
                while (string.IsNullOrWhiteSpace(opt.AdminConnection))
                {
                    Write("Enter the admin DB connection string: ");
                    opt.AdminConnection = ReadLine();

                    if (opt.AdminConnection == null)
                    {
                        throw new OperationCanceledException();
                    }
                }

                try
                {
                    adminConnBldr = new SqlConnectionStringBuilder(opt.AdminConnection);
                }
                catch
                {
                    throw new ArgumentException($"Invalid connection string \"{opt.AdminConnection}\"");
                }

                // Pre-Publish Script
                if (!string.IsNullOrWhiteSpace(opt.PrePublishScript) && !File.Exists(opt.PrePublishScript))
                {
                    throw new ArgumentException($"No Pre-Publish script found at \"{opt.PrePublishScript}\"");
                }

                // Backup Folder
                string workingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                opt.BackupFolder ??= Path.Combine(workingDirectory, "Backups");

                // To avoid negative batch size
                opt.BatchSize = Math.Max(opt.BatchSize, 1);
            }

            #endregion

            #region Tenants

            List<PublishWorkspace> workspaces;
            {
                var adminOpt = Options.Create(new AdminRepositoryOptions { ConnectionString = adminConnBldr.ConnectionString });
                var logger = new NullLogger<AdminRepository>();
                var repo = new AdminRepository(adminOpt, logger);
                var ctx = new QueryContext(0);

                WriteLine($"Loading tenants info from server \"{adminConnBldr.DataSource}\"...");
                var databases = await repo.SqlDatabases
                    .Expand(nameof(SqlDatabase.Server))
                    .OrderBy(nameof(SqlDatabase.Id))
                    .ToListAsync(ctx, cancellation);

                workspaces = databases.Select(db =>
                {
                    var connInfo = AdminRepositoryConnectionResolver.ToConnectionInfo(
                        serverName: db.Server.ServerName,
                        dbName: db.DatabaseName,
                        userName: db.Server.UserName,
                        _: db.Server.PasswordKey,
                        adminConnBuilder: adminConnBldr);

                    var dbConnBldr = new SqlConnectionStringBuilder
                    {
                        DataSource = connInfo.ServerName,
                        InitialCatalog = connInfo.DatabaseName,
                        UserID = connInfo.UserName,
                        Password = connInfo.Password,
                        IntegratedSecurity = connInfo.IsWindowsAuth,
                        PersistSecurityInfo = false,
                    };

                    return new PublishWorkspace
                    {
                        DbName = db.DatabaseName,
                        ConnectionString = dbConnBldr.ConnectionString
                    };
                })
                .ToList();

                Write($"\u2713 ", ConsoleColor.Green);
                Write($"Found DBs:");
                WriteLine($" [{string.Join("], [", workspaces.Select(w => w.DbName))}]", ConsoleColor.Cyan);
            }

            #endregion

            #region Confirmation

            if (!opt.SkipConfirmation)
            {
                string confirmed = "Confirmed";

                WriteLine();
                Write($"Confirm going ahead with all tenant DBs by typing ");
                Write($"\"{confirmed}\"");
                Write($": ");

                var answer = ReadLine();
                if (answer == null)
                {
                    throw new OperationCanceledException();
                }
                else if (answer.ToLower() != confirmed.ToLower())
                {
                    Write($"X", ConsoleColor.Red);
                    Write($" Did not confirm");
                    WriteLine();

                    return;
                }
                else
                {
                    Write($"\u2713 ", ConsoleColor.Green);
                    Write("Confirmation acquired.");
                    WriteLine();
                }
            }

            #endregion

            #region Backup Dir

            DateTime now = DateTime.Now;
            string backupsPath = null;

            if (!opt.SkipBackup)
            {
                string nowString = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                backupsPath = Path.Combine(opt.BackupFolder, nowString);

                Directory.CreateDirectory(backupsPath);

                Write($"\u2713 ", ConsoleColor.Green);
                Write($"Backup directory created at ");
                Write(backupsPath, ConsoleColor.Cyan);
                Write($".");
                WriteLine();
            }

            #endregion

            #region PrePublish Script

            string prePublishScript = null;
            if (!string.IsNullOrWhiteSpace(opt.PrePublishScript))
            {
                prePublishScript = await File.ReadAllTextAsync(opt.PrePublishScript, cancellation);
            }

            #endregion

            DacPackage dacPackage = null;
            try
            {
                #region DACPAC

                // Get the DACPAC package
                if (!opt.SkipPublish)
                {
                    dacPackage = DacPackage.Load(opt.DacpacFile);
                    {
                        // Sanity check just in case
                        string expectedName = "Tellma.Database.Application";
                        if (dacPackage.Name != expectedName)
                        {
                            throw new ArgumentException($"The DACPAC file \"{opt.DacpacFile}\" does not have the name {expectedName}.");
                        }

                        Write($"\u2713 ", ConsoleColor.Green);
                        WriteLine($"DACPAC version {dacPackage.Version} loaded.");
                    }
                }

                #endregion

                #region Backup and Publish

                WriteLine();
                WriteLine($"Operation started at {now:hh:mm:ss tt}...");

                foreach (var workspace in workspaces)
                {
                    workspace.Top = Console.CursorTop;
                    workspace.UpdateStatus("Getting ready");
                    WriteLine();
                }

                int skip = 0;
                while (!cancellation.IsCancellationRequested)
                {
                    var batch = workspaces.Skip(skip).Take(opt.BatchSize);
                    if (batch.Any())
                    {
                        await Task.WhenAll(batch.Select(async ws =>
                        {
                            try
                            {
                                #region DacService

                                var service = new DacServices(ws.ConnectionString);
                                service.Message += (object s, DacMessageEventArgs e) =>
                                {
                                    ws.UpdateStatus(e.Message);
                                };

                                #endregion

                                #region Backup

                                if (!opt.SkipBackup)
                                {
                                    // Export Package
                                    string bacpacPath = Path.Combine(backupsPath, $"{ws.DbName}.bacpac");
                                    await Task.Run(() => service.ExportBacpac(bacpacPath, ws.DbName, null, cancellation), cancellation);
                                }

                                #endregion

                                #region Pre-Publish Script

                                if (!string.IsNullOrWhiteSpace(opt.PrePublishScript))
                                {
                                    ws.UpdateStatus("Executing Pre-Publish Script (Started)");

                                    using var conn = new Microsoft.Data.SqlClient.SqlConnection(ws.ConnectionString);

                                    await Task.Run(() =>
                                    {
                                        Server server = new(new ServerConnection(conn));
                                        server.ConnectionContext.ExecuteNonQuery(prePublishScript);
                                    });

                                    ws.UpdateStatus("Executing Pre-Publish Script (Completed)");
                                }

                                #endregion

                                if (!opt.SkipPublish)
                                {
                                    #region DB Specs

                                    DacAzureDatabaseSpecification specs = null;
                                    {
                                        ws.UpdateStatus("Retrieving DB Specs (Started)");

                                        using var conn = new SqlConnection(ws.ConnectionString);
                                        using var cmd = conn.CreateCommand();
                                        cmd.CommandText = @$"
IF OBJECT_ID(N'sys.database_service_objectives') IS NOT NULL
SELECT 
	[edition] AS [Edition], 
	[service_objective] AS [ServiceObjective], 
	CAST(CAST(DATABASEPROPERTYEX(DB_Name(), 'MaxSizeInBytes') AS BIGINT) / (1024 * 1024 * 1024) AS INT) AS [MaximumSize]
FROM [sys].[database_service_objectives];

ALTER DATABASE [{ws.DbName}]
SET MULTI_USER;
";
                                        await conn.OpenAsync(cancellation);
                                        using var reader = await cmd.ExecuteReaderAsync(cancellation);

                                        if (await reader.ReadAsync(cancellation))
                                        {
                                            int i = 0;
                                            var editionString = reader.GetString(i++);
                                            if (!Enum.TryParse(editionString, out DacAzureEdition edition))
                                            {
                                                // Just in case
                                                throw new InvalidOperationException($"Unknown edition {editionString}");
                                            }

                                            specs = new DacAzureDatabaseSpecification
                                            {
                                                Edition = edition,
                                                ServiceObjective = reader.GetString(i++),
                                                MaximumSize = reader.GetInt32(i++)
                                            };
                                        }

                                        ws.UpdateStatus("Retrieving DB Specs (Completed)");
                                    }

                                    #endregion

                                    // Publish Options
                                    var options = new PublishOptions
                                    {
                                        GenerateDeploymentReport = false,
                                        GenerateDeploymentScript = false,
                                        CancelToken = cancellation,
                                        DeployOptions = new DacDeployOptions
                                        {
                                            IncludeCompositeObjects = true,
                                            BlockOnPossibleDataLoss = true,
                                            CreateNewDatabase = false,
                                            DropObjectsNotInSource = true,
                                            DeployDatabaseInSingleUserMode = !opt.SkipSingleUserMode,
                                            DatabaseSpecification = specs
                                        },
                                    };

                                    // SQLCMD Variables
                                    options.DeployOptions.SqlCommandVariableValues.Add("OverwriteDb", "0");
                                    options.DeployOptions.SqlCommandVariableValues.Add("DeployEmail", "NULL");
                                    options.DeployOptions.SqlCommandVariableValues.Add("FunctionalCurrency", "NULL");
                                    options.DeployOptions.SqlCommandVariableValues.Add("PrimaryLanguageId", "NULL");
                                    options.DeployOptions.SqlCommandVariableValues.Add("SecondaryLanguageId", "NULL");
                                    options.DeployOptions.SqlCommandVariableValues.Add("TernaryLanguageId", "NULL");
                                    options.DeployOptions.SqlCommandVariableValues.Add("ShortCompanyName", "NULL");
                                    options.DeployOptions.SqlCommandVariableValues.Add("ShortCompanyName2", "NULL");
                                    options.DeployOptions.SqlCommandVariableValues.Add("ShortCompanyName3", "NULL");

                                    // Publish DacPac
                                    await Task.Run(() =>
                                        {
                                            service.Publish(package: dacPackage, targetDatabaseName: ws.DbName, options);
                                        });
                                }

                                ws.UpdateStatus("Completed successfully", StatusType.Success);
                            }
                            catch (OperationCanceledException)
                            {
                                ws.UpdateStatus("Publish aborted", StatusType.Error);
                            }
                            catch (DacServicesException ex)
                            {
                                ws.ShowErrorReport = !cancellation.IsCancellationRequested;

                                ws.UpdateStatus(ex.Message, StatusType.Error);

                                ws.Warnings.AddRange(ex.Messages.Where(e => e.MessageType == DacMessageType.Warning).Select(e => e.Message));
                                ws.Errors.AddRange(ex.Messages.Where(e => e.MessageType == DacMessageType.Error).Select(e => e.Message));
                                ws.Errors.Add(ex.ToString());

                            }
                            catch (Exception ex)
                            {
                                ws.ShowErrorReport = !cancellation.IsCancellationRequested;

                                ws.UpdateStatus(ex.Message, StatusType.Error);
                                ws.Errors.Add(ex.ToString());
                            }
                        }));

                        skip += opt.BatchSize;
                    }
                    else
                    {
                        break;
                    }
                }


                WriteLine($"Operation completed at {DateTime.Now:hh:mm:ss tt}.");

                // Error reports
                foreach (var ws in workspaces.Where(e => e.ShowErrorReport))
                {
                    WriteLine();
                    WriteLine();
                    WriteLine($"------- [{ws.DbName}] Errors --------");
                    foreach (var error in ws.Errors)
                    {
                        WriteLine(error, ConsoleColor.Red);
                    }
                }

                #endregion
            }
            finally
            {
                if (dacPackage != null)
                {
                    dacPackage.Dispose();
                }
            }
        }

        #region Helpers

        private enum StatusType
        {
            Regular = 0,
            Success = 1,
            Error = 2
        }

        private class PublishWorkspace
        {
            public string ConnectionString { get; set; }
            public string DbName { get; set; }
            public bool ShowErrorReport { get; set; }
            public int Top { get; set; }

            public List<string> Warnings { get; } = new List<string>();
            public List<string> Errors { get; } = new List<string>();

            public void UpdateStatus(string msg, StatusType type = StatusType.Regular)
            {
                var color = type switch
                {
                    StatusType.Error => ConsoleColor.Red,
                    StatusType.Success => ConsoleColor.Green,
                    _ => ConsoleColor.Cyan
                };

                var prefix = type switch
                {
                    StatusType.Error => $"X ",
                    StatusType.Success => $"\u2713 ",
                    _ => $". "
                };

                string dbName = $"[{DbName}] ";

                lock (_sync)
                {
                    int availableWidth = Console.WindowWidth - prefix.Length - dbName.Length - 1;
                    msg = msg.Split(Environment.NewLine)?.FirstOrDefault()?.Truncate(availableWidth); // Take the first line
                    msg += new string(' ', availableWidth - msg.Length); // To erase the rest of the Console line

                    var (originalLeft, originalTop) = Console.GetCursorPosition();
                    Console.SetCursorPosition(0, Top);

                    WriteNoLock(prefix, color);
                    WriteNoLock(dbName);
                    WriteNoLock(msg, color);

                    Console.SetCursorPosition(originalLeft, originalTop);
                }
            }

            public void UpdateStatus(DacMessage msg) => UpdateStatus(msg.Message);
        }

        private static readonly object _sync = new();

        private static void WriteNoLock(string msg, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.Write(msg);
            Console.ResetColor();
        }

        private static void Write(string msg, ConsoleColor color = ConsoleColor.White)
        {
            lock (_sync)
            {
                Console.ForegroundColor = color;
                Console.Write(msg);
                Console.ResetColor();
            }
        }

        private static void WriteLine()
        {
            lock (_sync)
            {
                Console.WriteLine();
            }
        }

        private static void WriteLine(string msg, ConsoleColor color = ConsoleColor.White)
        {
            lock (_sync)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(msg);
                Console.ResetColor();
            }
        }

        private static string ReadLine()
        {
            lock (_sync)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                var answer = Console.ReadLine();
                Console.ResetColor();

                return answer;
            }
        }

        #endregion
    }
}
