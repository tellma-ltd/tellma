using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Tellma.Repository.Admin;
using Tellma.Services.Utilities;

namespace Tellma
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Initialize the database
            try
            {
                await InitDatabase(host.Services);
                host.Services.GetRequiredService<ILogger<Program>>().LogInformation("Tellma Web Server Started.");
            }
            catch (Exception ex)
            {
                Startup.StartupError = ex.Message;
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBldr = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .ConfigureLogging((ctx, loggingBldr) =>
                {
                    loggingBldr.AddDebug();
                    loggingBldr.AddAzureWebAppDiagnostics(); // For Azure, has no effect elsewhere

                    // Sends logged errors to an email address specified in the settings.
                    loggingBldr.AddEmail(ctx.Configuration);
                });

            return hostBldr;
        }

        /// <summary>
        /// Database initialization is performed here, after the web host is configured but before it is run
        /// this way the initialization has access to environment variables in configuration providers, but it
        /// only runs once when the web app loads.
        /// </summary>
        public static async Task InitDatabase(IServiceProvider provider)
        {
            // If missing, the default admin user is added here
            using var scope = provider.CreateScope();

            // (1) Retrieve the admin credentials from configurations
            var opt = scope.ServiceProvider.GetRequiredService<IOptions<AdminOptions>>().Value;
            string email = opt.Email ?? "admin@tellma.com";
            string fullName = opt.FullName ?? "Administrator";
            string password = opt.Password ?? "Admin@123";

            // (2) Create the user in the admin database
            var adminRepo = scope.ServiceProvider.GetRequiredService<AdminRepository>();
            await adminRepo.AdminUsers__CreateAdmin(email, fullName);

            // (3) Create the user in the identity server (if possible)
            var identity = scope.ServiceProvider.GetRequiredService<Api.IIdentityProxy>();
            if (identity.CanCreateUsers)
            {
                var singleton = new System.Collections.Generic.List<string> { email };
                await identity.CreateUsersIfNotExist(singleton);
            }
        }
    }
}
