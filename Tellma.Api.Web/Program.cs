using Microsoft.AspNetCore.Hosting;
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
            }
            catch (Exception ex)
            {
                Startup.StartupError = ex.Message;
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddDebug();
                logging.AddAzureWebAppDiagnostics(); // For Azure, has no effect elsewhere

                // TODO: Enable and Test (Take into account that email service may log errors too
                //logging.AddEmailLogger(opt => // Sends an email to support when an unhandled error happens
                //{
                //    hostingContext.Configuration.GetSection("Logging").GetSection("Email").Bind(opt);
                //});
            });

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
