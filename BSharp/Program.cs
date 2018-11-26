using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BSharp.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BSharp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            // In development mode, apply migrations and seed the database
            using (var scope = host.Services.CreateScope())
            {
                var env = scope.ServiceProvider.GetRequiredService<IHostingEnvironment>();
                if (env.IsDevelopment())
                {
                    try
                    {
                        scope.ServiceProvider.GetRequiredService<ShardingContext>().Database.Migrate();
                        scope.ServiceProvider.GetRequiredService<ApplicationContext>().Database.Migrate();
                     //   scope.ServiceProvider.GetRequiredService<ConfigurationContext>().Database.Migrate();
                        scope.ServiceProvider.GetRequiredService<LocalizationContext>().Database.Migrate();
                      //  scope.ServiceProvider.GetRequiredService<IdentityContext>().Database.Migrate();
                    }
                    catch (Exception ex)
                    {
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred while migrating or seeding the databases.");
                    }
                }
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        public static IWebHost BuildWebHost(string[] args) => CreateWebHostBuilder(args).Build();
    }
}
