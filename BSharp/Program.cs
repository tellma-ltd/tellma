using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BSharp.Data;
using BSharp.Services.Sharding;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();

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
                        // (1) Configuration Context migrated the usual way
                        //   scope.ServiceProvider.GetRequiredService<ConfigurationContext>().Database.Migrate();


                        // (2) Sharding Context migrated the usual way
                        scope.ServiceProvider.GetRequiredService<ShardingContext>().Database.Migrate();


                        // (3) Identity Context migrated the usual way
                        scope.ServiceProvider.GetRequiredService<IdentityContext>().Database.Migrate();


                        // (4) Application Context requires special handling in development, don't resolve it with DI
                        var shardResolver = scope.ServiceProvider.GetRequiredService<IShardResolver>();
                        var appContext = new ApplicationContext(shardResolver, 
                            new DesignTimeTenantIdProvider(), new DesignTimeUserIdProvider());

                        appContext.Database.Migrate();


                        // (5) Localization Context migration and seeding
                        var localizationCtx = scope.ServiceProvider.GetRequiredService<LocalizationContext>();
                        localizationCtx.Database.Migrate();

                        // Translations are seeded here for a better development experience since they change 
                        // frequently, in the future this seeding will be moved to migrations instead
                        var dbTranslations = localizationCtx.CoreTranslations.ToList();
                        localizationCtx.Database.ExecuteSqlCommand("TRUNCATE TABLE CoreTranslations");
                        localizationCtx.CoreTranslations.AddRange(LocalizationContext._TRANSLATIONS);
                        localizationCtx.SaveChanges();
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
