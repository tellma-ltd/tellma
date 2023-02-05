using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Dac;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
                await CloneDatabase(args, cancellation);
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

            if (string.IsNullOrWhiteSpace(opt.ConnectionString))
            {
                throw new ArgumentException($"The parameter {nameof(opt.ConnectionString)} is required.");
            }

            while (string.IsNullOrWhiteSpace(opt.Source))
            {
                Write("Enter Source DB Name (e.g. Tellma.123): ");
                opt.Source = ReadLine();
                WriteLine();
            }

            while (string.IsNullOrWhiteSpace(opt.Destination))
            {
                Write("Enter Destination DB Name (e.g. Tellma.456): ");
                opt.Destination = ReadLine();
                WriteLine();
            }

            #endregion

            #region Connection Strings

            string neutralConnString;
            string sourceConnString;
            string destinationConnString;
            {
                var bldr = new SqlConnectionStringBuilder(opt.ConnectionString)
                {
                    InitialCatalog = ""
                };

                neutralConnString = bldr.ConnectionString;

                // Source
                bldr.InitialCatalog = opt.Source;
                sourceConnString = bldr.ConnectionString;

                // Destination
                bldr.InitialCatalog = opt.Destination;
                destinationConnString = bldr.ConnectionString;
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
                    WriteLine($"============= Exporting from {opt.Source}...");
                    service.ExportBacpac(bacpacPath, opt.Source, null, cancellation);
                    WriteLine($"\u2713 Exporting {opt.Source} is complete.", ConsoleColor.Green);
                    WriteLine();
                }

                // Import package in destination
                {
                    WriteLine($"============= Importing into {opt.Destination}...");
                    var package = BacPackage.Load(bacpacPath);
                    service.ImportBacpac(package, opt.Destination, new DacAzureDatabaseSpecification { Edition = DacAzureEdition.Basic }, cancellation);
                    WriteLine($"\u2713 Importing into {opt.Destination} is complete.", ConsoleColor.Green);
                }

                // Changing the navbar color
                {
                    using var conn = new SqlConnection(destinationConnString);
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = @$"UPDATE [dbo].[Settings] SET [BrandColor] = N'#5c5c5c', [SettingsVersion] = NEWID();";
                    await conn.OpenAsync(cancellation);
                    await cmd.ExecuteNonQueryAsync(cancellation);
                    WriteLine($"\u2713 Updated {opt.Destination} brand color to gray.", ConsoleColor.Green);
                    WriteLine();
                }
            }
            finally
            {
                File.Delete(bacpacPath);
            }

            #region Launch Chrome

            if (!string.IsNullOrWhiteSpace(opt.LaunchUrl))
            {
                Process process = new();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = "chrome";
                process.StartInfo.Arguments = opt.LaunchUrl;
                process.Start();
            }

            #endregion
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
