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

                        // Add first user
                        var cmd = appContext.Database.GetDbConnection().CreateCommand();


                        var now = DateTimeOffset.Now;
                        appContext.Database.ExecuteSqlCommand(
                            @"
DECLARE @NextId INT = IDENT_CURRENT('[dbo].[LocalUsers]') + 1;
INSERT INTO [dbo].[LocalUsers] (Email, ExternalId, CreatedAt, ModifiedAt, Name, Name2, CreatedById, ModifiedById, TenantId)
                            VALUES ({0}, {1}, {2}, {2}, {3}, {4}, @NextId, @NextId, 101)",
                            "support@banan-it.com", // {0}
                            "4F7785F2-5942-4CFB-B5AD-85AB72F7EB35", // {1}
                            now, // {2}
                            "Banan IT Support", // {3}
                            "فريق مساندة بنان"); // {4}


                        //// Add the views
                        //string[] viewIds = { "measurement-units", "individuals", "organizations", "roles" };
                        //foreach(var viewId in viewIds)
                        //{
                        //    if(!appContext.Views.Any(e => e.Id == viewId))
                        //    {
                        //        appContext.Views.Add(new View { Id = viewId, IsActive = true });
                        //    }
                        //}

                        appContext.SaveChanges();
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
