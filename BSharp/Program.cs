using BSharp.Data;
using BSharp.Data.Model;
using BSharp.Services.Sharding;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using static BSharp.Data.ApplicationContext;

namespace BSharp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            MigrateAndSeedDatabasesInDevelopment(host);

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args).UseStartup<Startup>()
            .ConfigureLogging((hostingContext, logging) => logging.AddDebug());

        public static IWebHost BuildWebHost(string[] args) =>
            CreateWebHostBuilder(args).Build();

        /// <summary>
        /// Migrates and seeds the database in development environment, 
        /// calling this method in production doesn't do anything
        /// </summary>
        /// <param name="host"></param>
        private static void MigrateAndSeedDatabasesInDevelopment(IWebHost host)
        {
            // In development mode, apply migrations and seed the database
            using (var scope = host.Services.CreateScope())
            {
                var env = scope.ServiceProvider.GetRequiredService<IHostingEnvironment>();
                if (env.IsDevelopment())
                {
                    try
                    {
                        // (1) Admin Context migrated the usual way, add one tenant for dev and all translations
                        var adminContext = scope.ServiceProvider.GetRequiredService<AdminContext>();
                        adminContext.Database.Migrate();
                        if (!adminContext.Tenants.Any())
                        {
                            adminContext.Tenants.Add(new Tenant
                            {
                                Id = 101,
                                Name = "Contoso, Inc.",
                                ShardId = 1
                            });

                            adminContext.Tenants.Add(new Tenant
                            {
                                Id = 102,
                                Name = "Fabrikam & Co.",
                                ShardId = 1
                            });

                            adminContext.SaveChanges();
                        }

                        // Translations are seeded here for a better development experience since they change 
                        // frequently, in the future this seeding will be moved to migrations instead
                        adminContext.Database.ExecuteSqlCommand("DELETE FROM [dbo].[Translations]");

                        adminContext.Translations.AddRange(Translation.TRANSLATIONS);
                        adminContext.SaveChanges();


                        // (2) Application Context requires special handling in development, don't resolve it with DI
                        var shardResolver = scope.ServiceProvider.GetRequiredService<IShardResolver>();
                        var appContext = new ApplicationContext(shardResolver,
                            new DesignTimeTenantIdProvider(), new DesignTimeUserIdProvider());

                        appContext.Database.Migrate();
                    }
                    catch (Exception ex)
                    {
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred while migrating or seeding the databases.");
                    }
                }
            }
        }
    }
}
