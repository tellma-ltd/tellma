using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using Tellma.Data;
using Tellma.Services.EmbeddedIdentityServer;
using Tellma.Services.Utilities;

namespace Tellma
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Initialize the database
            try { InitDatabase(host.Services); }
            catch (Exception ex)
            {
                Startup.GlobalError = $"{ex.GetType().Name}: {ex.Message}";
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
        /// only runs once when the web app loads
        /// </summary>
        public static void InitDatabase(IServiceProvider provider)
        {
            // If missing, the default admin user is added here
            using var scope = provider.CreateScope();

            // (1) Retrieve the admin credentials from configurations
            var opt = scope.ServiceProvider.GetRequiredService<IOptions<GlobalOptions>>().Value;
            string email = opt?.Admin?.Email ?? "admin@tellma.com";
            string fullName = opt?.Admin?.FullName ?? "Administrator";
            string password = opt?.Admin?.Password ?? "Admin@123";

            // (2) Create the user in the admin database
            var adminRepo = scope.ServiceProvider.GetRequiredService<AdminRepository>();
            adminRepo.AdminUsers__CreateAdmin(email, fullName, password).Wait();

            // (3) Create the user in the embedded identity server (if enabled)
            if (opt.EmbeddedIdentityServerEnabled)
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<EmbeddedIdentityServerUser>>();
                var admin = userManager.FindByEmailAsync(email).GetAwaiter().GetResult();

                if (admin == null)
                {
                    admin = new EmbeddedIdentityServerUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };

                    var result = userManager.CreateAsync(admin, password).GetAwaiter().GetResult();
                    if (!result.Succeeded)
                    {
                        string msg = string.Join(", ", result.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to create the administrator account. Message: {msg}");
                    }
                }
            }
        }
    }
}
