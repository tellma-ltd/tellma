using BSharp.Data;
using BSharp.Services.EmbeddedIdentityServer;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace BSharp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            // Initialize the database
            try { InitDatabase(host.Services); }
            catch (Exception ex)
            {
                Startup.GlobalError = $"{ex.GetType().Name}: {ex.Message}";
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .ConfigureLogging((hostingContext, logging) => logging.AddDebug());

        /// <summary>
        /// Database initialization is performed here, after the web host is configured but before it is run
        /// this way the initialization has access to environment variables in configuration providers, but it
        /// only runs once when the web app loads
        /// </summary>
        public static void InitDatabase(IServiceProvider provider)
        {
            // If missing, the default admin user is added here
            using (var scope = provider.CreateScope())
            {
                // (1) Retrieve the admin credentials from configurations
                var opt = scope.ServiceProvider.GetRequiredService<IOptions<GlobalOptions>>().Value;
                string email = opt?.Admin?.Email ?? "admin@bsharp.online";
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
}
