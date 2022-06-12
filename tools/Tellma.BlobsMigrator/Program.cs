using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Repository.Admin;
using Tellma.Repository.Application;
using Tellma.Utilities.Blobs;
using Tellma.Utilities.Sharding;

namespace Tellma.BlobsMigrator
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

            WriteLine($"Started at {DateTime.Now}.");
            Write("Press ");
            Write("Ctrl+C", ConsoleColor.DarkYellow);
            Write(" to cancel anytime...");
            WriteLine();
            WriteLine();

            #endregion

            try
            {
                await MigrateBlobs(args, cancellation);
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

        private static async Task MigrateBlobs(string[] args, CancellationToken cancellation)
        {
            #region Arguments

            MigratorOptions opt;
            SqlConnectionStringBuilder adminConnBldr;
            {
                // Build the configuration root based on project user secrets
                IConfiguration config = new ConfigurationBuilder()
                    .AddUserSecrets(typeof(Program).Assembly)
                    .AddCommandLine(args) // Higher precedence
                    .Build();

                opt = config.Get<MigratorOptions>();

                // TenantId

                if (string.IsNullOrWhiteSpace(opt.AdminConnection))
                {
                    throw new ArgumentException($"Argument 'AdminConnection' is required.");
                }

                if (opt.TenantId == 0)
                {
                    throw new ArgumentException($"Argument 'TenantId' is required.");
                }

                if (string.IsNullOrWhiteSpace(opt.BlobStorageConnectionString))
                {
                    throw new ArgumentException($"Argument 'BlobStorageConnectionString' is required.");
                }

                if (string.IsNullOrWhiteSpace(opt.BlobStorageContainerName))
                {
                    throw new ArgumentException($"Argument 'BlobStorageContainerName' is required.");
                }

                // Admin connection
                try
                {
                    adminConnBldr = new SqlConnectionStringBuilder(opt.AdminConnection);
                }
                catch
                {
                    throw new ArgumentException($"Invalid connection string \"{opt.AdminConnection}\"");
                }
            }

            #endregion

            // Get the tenant's connection string
            SqlConnectionStringBuilder dbConnBldr;
            {
                var adminOpt = Options.Create(new AdminRepositoryOptions { ConnectionString = adminConnBldr.ConnectionString });
                var logger = new NullLogger<AdminRepository>();
                var repo = new AdminRepository(adminOpt, logger);

                var (serverName, dbName, userName, passwordKey) = await repo.GetDatabaseConnectionInfo(opt.TenantId, cancellation);
                var connInfo = AdminRepositoryConnectionResolver.ToConnectionInfo(serverName, dbName, userName, passwordKey, adminConnBldr);

                dbConnBldr = new SqlConnectionStringBuilder
                {
                    DataSource = connInfo.ServerName,
                    InitialCatalog = connInfo.DatabaseName,
                    UserID = connInfo.UserName,
                    Password = connInfo.Password,
                    IntegratedSecurity = connInfo.IsWindowsAuth,
                    PersistSecurityInfo = false,
                };
            }

            Write($"Loading blob Ids from ");
            Write(dbConnBldr.InitialCatalog, ConsoleColor.Cyan);
            WriteLine("...");
            string connString = dbConnBldr.ConnectionString;
            List<string> blobNames = new();
            {
                using var conn = new SqlConnection(connString);
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @$"SELECT [Id] FROM [dbo].[Blobs];";
                await conn.OpenAsync(cancellation);
                using var reader = await cmd.ExecuteReaderAsync(cancellation);

                while (await reader.ReadAsync(cancellation))
                {
                    var blobName = reader.GetString(0);
                    blobNames.Add(blobName);
                }

                Write("Found ");
                Write(blobNames.Count + " ", ConsoleColor.Green);
                WriteLine("blobs.");
            }

            if (blobNames.Count > 0)
            {
                #region Confirmation

                {
                    WriteLine();
                    Write($"Confirm migrating ");
                    Write(blobNames.Count.ToString(), ConsoleColor.Green);
                    Write($" blobs from ");
                    Write(dbConnBldr.InitialCatalog, ConsoleColor.Cyan);
                    Write($" to Blob Container ");
                    Write(opt.BlobStorageContainerName, ConsoleColor.Cyan);
                    Write($" by entering \"y\": ");

                    var answer = ReadLine();
                    if (answer?.ToLower() != "y")
                    {
                        Write($"X", ConsoleColor.Red);
                        WriteLine($" Did not confirm");

                        return;
                    }

                    WriteLine();
                }

                #endregion


                var sqlBlobs = new SqlBlobService(
                    factory: new ApplicationRepositoryFactory(
                        logger: new NullLogger<ApplicationRepository>(),
                        shardResolver: new ShardResolver(connString, opt.TenantId)));


                var azureBlobs = new AzureBlobStorageService(
                    config: Options.Create(new AzureBlobStorageOptions
                    {
                        ConnectionString = opt.BlobStorageConnectionString,
                        ContainerName = opt.BlobStorageContainerName
                    }));


                int counter = -1;
                object counterLock = new();
                WriteLine();
                void IncrementCounterAndReportProgress() // Thread-safe function
                {
                    lock (counterLock)
                    {
                        counter++;
                        var (left, top) = Console.GetCursorPosition();
                        Console.SetCursorPosition(0, top - 1);
                        WriteLine($"Migrated {counter}/{blobNames.Count} blobs.");
                    }
                }

                IncrementCounterAndReportProgress(); // Start with 0;
                foreach (var batch in blobNames.Batch(size: 10))
                {
                    await Task.WhenAll(batch.Select(async blobName =>
                    {
                        var blobBytes = await sqlBlobs.LoadBlobAsync(opt.TenantId, blobName, cancellation);

                        try
                        {
                            await azureBlobs.SaveBlobsAsync(opt.TenantId, new List<(string, byte[])> { (blobName, blobBytes) });
                        }
                        catch (Azure.RequestFailedException ex) when (ex.ErrorCode == "BlobAlreadyExists")
                        {
                            // Update
                            await azureBlobs.DeleteBlobsAsync(opt.TenantId, new List<string> { blobName });
                            await azureBlobs.SaveBlobsAsync(opt.TenantId, new List<(string, byte[])> { (blobName, blobBytes) });
                        }

                        await sqlBlobs.DeleteBlobsAsync(opt.TenantId, new List<string> { blobName });

                        IncrementCounterAndReportProgress();
                    }));
                }
            }

            WriteLine($"Finished at {DateTime.Now}.");
        }

        #region Helpers

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

        #endregion
    }

    public static class EnumExtensions
    {
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int size)
        {
            if (size <= 0)
            {
                throw new ArgumentException("Argument must be greater than zero.", nameof(size));
            }

            List<T> bucket = new(size);
            foreach (var item in source)
            {
                bucket.Add(item);
                if (bucket.Count == size)
                {
                    yield return bucket.Select(x => x);
                    bucket.Clear();
                }
            }

            // Return the last bucket with all remaining elements
            if (bucket.Count > 0)
            {
                yield return bucket.Select(x => x);
            }
        }
    }

    public class ShardResolver : IShardResolver
    {
        private readonly string _connString;
        private readonly int _databaseId;

        public ShardResolver(string connString, int databaseId)
        {
            _connString = connString;
            _databaseId = databaseId;
        }

        public Task<string> GetConnectionString(int databaseId, CancellationToken cancellation)
        {
            if (_databaseId != databaseId)
            {
                throw new InvalidOperationException("Wrong databaseId supplied"); // Just to make it fail proof
            }

            return Task.FromResult(_connString);
        }
    }
}
