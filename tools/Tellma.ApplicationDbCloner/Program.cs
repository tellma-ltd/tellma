using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Model.Admin;
using Tellma.Repository.Admin;
using Tellma.Repository.Common;

namespace Tellma.ApplicationDbCloner
{
    class Program
    {
        static async Task Main(string[] args)
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
                var startTime = DateTime.Now;
                await CloneDatabase(args, cancellation);
                
                var finishTime = DateTime.Now;
                WriteLine($"Started at {startTime}.");
                WriteLine($"Finished at {finishTime}.");
            }
            catch (OperationCanceledException)
            {
                WriteLine("Publish aborted.", ConsoleColor.DarkYellow);
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString(), ConsoleColor.Red);
            }

            if (!cancellation.IsCancellationRequested)
            {
                WriteLine();
                Write("Press Enter to exit...");
                ReadLine();
            }
        }

        private static async Task CloneDatabase(string[] args, CancellationToken cancellation)
        {
            // For some reason we're getting a weird error on some machine:
            //      "4096 (0x1000) is an invalid culture identifier."
            // So following might be a workaround according to: https://github.com/dotnet/runtime/issues/60296
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            System.Globalization.CultureInfo.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

            #region Arguments

            // Build the configuration root based on project user secrets
            IConfiguration config = new ConfigurationBuilder()
                .AddUserSecrets(typeof(Program).Assembly)
                .AddCommandLine(args) // Higher precedence
                .Build();

            var opt = config.Get<ClonerOptions>();

            if (string.IsNullOrWhiteSpace(opt.AdminConnection))
            {
                throw new ArgumentException($"The parameter {nameof(opt.AdminConnection)} is required.");
            }

            while (opt.SourceId <= 0)
            {
                Write("Enter source tenant Id (e.g. 101): ");
                if (int.TryParse(ReadLine(), out int source))
                {
                    opt.SourceId = source;
                }
                else
                {
                    Write("Please enter an integer greater than zero.");
                }
            }

            while (opt.DestinationId <= 0)
            {
                Write("Enter an unused destination tenant Id (e.g. 1101): ");
                if (int.TryParse(ReadLine(), out int destination))
                {
                    opt.DestinationId = destination;
                }
                else
                {
                    Write("Please enter an integer greater than zero.");
                }
            }

            #endregion

            #region Connection Strings

            string neutralConnString;
            string adminDbName;
            string adminConnString;
            string sourceDbName;
            string sourceConnString;
            string destinationDbName;
            string destinationConnString;
            {
                // Admin
                var adminConnBldr = new SqlConnectionStringBuilder(opt.AdminConnection);
                adminConnString = adminConnBldr.ConnectionString;
                adminDbName = adminConnBldr.InitialCatalog;

                // Source
                var adminOpt = Options.Create(new AdminRepositoryOptions { ConnectionString = adminConnString });
                var logger = new NullLogger<AdminRepository>();
                var repo = new AdminRepository(adminOpt, logger);
                var ctx = new QueryContext(0);

                WriteLine($"Loading source tenant info from [{adminDbName}]...");
                var sourceDb = await repo.SqlDatabases
                    .Expand(nameof(SqlDatabase.Server))
                    .Filter($"{nameof(SqlDatabase.Id)} = {opt.SourceId}")
                    .FirstOrDefaultAsync(ctx, cancellation);

                if (sourceDb == null)
                {
                    throw new InvalidOperationException($"Source tenant Id {opt.SourceId} was not found in [{adminConnBldr.InitialCatalog}].[{nameof(repo.SqlDatabases)}]");
                }

                var destinationDb = await repo.SqlDatabases
                    .Expand(nameof(SqlDatabase.Server))
                    .Filter($"{nameof(SqlDatabase.Id)} = {opt.DestinationId}")
                    .FirstOrDefaultAsync(ctx, cancellation);

                if (destinationDb != null)
                {
                    throw new InvalidOperationException($"Destination tenant Id {opt.DestinationId} already exists in [{adminConnBldr.InitialCatalog}].[{nameof(repo.SqlDatabases)}]");
                }

                var connInfo = AdminRepositoryConnectionResolver.ToConnectionInfo(
                    serverName: sourceDb.Server.ServerName,
                    dbName: sourceDb.DatabaseName,
                    userName: sourceDb.Server.UserName,
                    _: sourceDb.Server.PasswordKey,
                    adminConnBuilder: adminConnBldr);

                var sourceConnBldr = new SqlConnectionStringBuilder
                {
                    DataSource = connInfo.ServerName,
                    InitialCatalog = connInfo.DatabaseName,
                    UserID = connInfo.UserName,
                    Password = connInfo.Password,
                    IntegratedSecurity = connInfo.IsWindowsAuth,
                    PersistSecurityInfo = false,
                    ConnectTimeout = 120
                };

                sourceConnString = sourceConnBldr.ConnectionString;
                sourceDbName = sourceConnBldr.InitialCatalog;

                // Destination
                var destinationConnBldr = new SqlConnectionStringBuilder(sourceConnBldr.ConnectionString)
                {
                    InitialCatalog = $"Tellma.{opt.DestinationId}"
                };

                destinationConnString = destinationConnBldr.ConnectionString;
                destinationDbName = destinationConnBldr.InitialCatalog;

                // Neutral
                var neutralConnBldr = new SqlConnectionStringBuilder(sourceConnBldr.ConnectionString)
                {
                    InitialCatalog = ""
                };

                neutralConnString = neutralConnBldr.ConnectionString;
            }

            #endregion

            // Initial setup
            var service = new DacServices(neutralConnString);
            service.Message += (object s, DacMessageEventArgs e) =>
            {
                WriteLine(e.Message.Message);
            };

            string bacpacPath = $"{Guid.NewGuid():N}.bacpac";
            try
            {
                WriteLine();
                WriteLine("Version 1.4");
                WriteLine();

                // Exporting package from source
                {
                    WriteLine();

                    Write($"============= Exporting ");
                    Write($"[{sourceDbName}]", ConsoleColor.Cyan);
                    WriteLine("...");

                    service.ExportBacpac(bacpacPath, sourceDbName, null, cancellation);

                    Write($"\u2713 ", ConsoleColor.Green);
                    Write($"Exporting ");
                    Write($"[{sourceDbName}] ", ConsoleColor.Cyan);
                    WriteLine($"is complete.");

                    WriteLine();
                }

                // Importing package in destination
                {

                    Write($"============= Importing into ");
                    Write($"[{destinationDbName}]", ConsoleColor.Cyan);
                    WriteLine("...");

                    var package = BacPackage.Load(bacpacPath);
                    service.ImportBacpac(package, destinationDbName, new DacAzureDatabaseSpecification { Edition = DacAzureEdition.Basic }, cancellation);

                    Write($"\u2713 ", ConsoleColor.Green);
                    Write($"Importing into ");
                    Write($"[{destinationDbName}] ", ConsoleColor.Cyan);
                    WriteLine($"is complete.");

                    WriteLine();
                }

                // Update admin DB
                {
                    Write($"============= In ");
                    Write($"[{adminDbName}]", ConsoleColor.Cyan);
                    WriteLine("...");
                    {
                        string updateSqlDatabasesCmdTxt = @$"INSERT INTO [dbo].[SqlDatabases] ([Id], [DatabaseName], [ServerId], [Description], [CreatedById], [ModifiedById])
SELECT {opt.DestinationId}, N'{destinationDbName}', [ServerId], N'Clone of ' + [Description], [CreatedById], [ModifiedById]
FROM [dbo].[SqlDatabases]
WHERE [Id] = {opt.SourceId}";

                        await ExecuteNonQuery(updateSqlDatabasesCmdTxt, adminConnString, cancellation);

                        Write($"\u2713 ", ConsoleColor.Green);
                        WriteLine($"Updated [SqlDatabases].");
                    }

                    if (!opt.SkipDirectoryUserMemberships)
                    {
                        // Grab the users from the des
                        string updateMembershipsCmdTxt = $@"INSERT INTO [dbo].[DirectoryUserMemberships] ([UserId], [DatabaseId])
SELECT [UserId], {opt.DestinationId}
FROM [dbo].[DirectoryUserMemberships]
WHERE [DatabaseId] = {opt.SourceId}";

                        await ExecuteNonQuery(updateMembershipsCmdTxt, adminConnString, cancellation);
                        Write($"\u2713 ", ConsoleColor.Green);
                        WriteLine($"Updated [DirectoryUserMemberships].");
                    }

                    WriteLine();
                }

                // Post-cloning script on destination
                {
                    Write($"============= In ");
                    Write($"[{destinationDbName}]", ConsoleColor.Cyan);
                    WriteLine("...");

                    // Run post-clone script
                    {
                        string postCloneScriptPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DestinationPostCloningScript.sql");
                        string postCloneScript = File.ReadAllText(postCloneScriptPath);

                        using var conn = new Microsoft.Data.SqlClient.SqlConnection(destinationConnString);
                        Server server = new(new ServerConnection(conn));
                        server.ConnectionContext.ExecuteNonQuery(postCloneScript);

                        Write($"\u2713 ", ConsoleColor.Green);
                        WriteLine($"Updated brand color to gray.");
                        Write($"\u2713 ", ConsoleColor.Green);
                        WriteLine($"Renamed company X to Clone of X.");
                        Write($"\u2713 ", ConsoleColor.Green);
                        WriteLine($"Changed all users to state 'New'.");
                        Write($"\u2713 ", ConsoleColor.Green);
                        WriteLine($"Deleted all image references and attachment metadata.");
                        Write($"\u2713 ", ConsoleColor.Green);
                        WriteLine($"Deleted email and SMS logs.");
                        Write($"\u2713 ", ConsoleColor.Green);
                        WriteLine($"Disabled all automatic email and SMS notifications.");
                    }

                    // Move the cloned DB to the elastic pool
                    {
                        // Get elastic pool name from the source
                        string poolName = null;
                        {
                            string getPoolNameCmdTxt = @"SELECT [dso].[elastic_pool_name]
FROM [sys].[databases] [D]
JOIN [sys].[database_service_objectives] dso 
ON [d].[database_id] = [dso].[database_id]
WHERE d.[name] = @DbName";

                            using var conn = new SqlConnection(sourceConnString);
                            using var cmd = conn.CreateCommand();
                            cmd.CommandText = getPoolNameCmdTxt;
                            cmd.Parameters.AddWithValue("@DbName", sourceDbName);
                            await conn.OpenAsync(cancellation);
                            var reader = await cmd.ExecuteReaderAsync(cancellation);
                            if (reader.Read())
                            {
                                poolName = reader.GetString(0);
                            }
                        }

                        try
                        {
                            // If the original is in a specific pool, add the clone to it too
                            if (!string.IsNullOrWhiteSpace(poolName))
                            {
                                string updateTierCmdTxt = @$"ALTER DATABASE [{destinationDbName}]
	MODIFY ( SERVICE_OBJECTIVE = ELASTIC_POOL ( name = [{poolName}]) );";

                                await ExecuteNonQuery(updateTierCmdTxt, destinationConnString, cancellation);

                                Write($"\u2713 ", ConsoleColor.Green);
                                WriteLine($"Added database to elastic pool [{poolName}].");
                            }
                        }
                        catch
                        {
                            WriteLine($"\u26A0 Failed to move the clone database to elastic pool [{poolName}], please move it manually.", ConsoleColor.Yellow);
                        }
                    }
                }

                WriteLine();
            }
            finally
            {
                File.Delete(bacpacPath);
            }

            #region Launch Chrome

            {
                string url = $"https://web.tellma.com/app/{opt.DestinationId}/main-menu";

                Process process = new();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = "chrome";
                process.StartInfo.Arguments = url;
                process.Start();
            }

            #endregion
        }

        private static async Task ExecuteNonQuery(string command, string connString, CancellationToken cancellation)
        {
            using var conn = new SqlConnection(connString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = command;
            await conn.OpenAsync(cancellation);
            await cmd.ExecuteNonQueryAsync(cancellation);
        }

        private static readonly object _sync = new();

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
    }
}
