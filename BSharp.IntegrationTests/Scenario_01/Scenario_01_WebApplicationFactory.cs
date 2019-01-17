using BSharp.Data;
using BSharp.Data.Model;
using BSharp.IntegrationTests.Utilities;
using BSharp.Services.Sharding;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using static BSharp.Data.ApplicationContext;

namespace BSharp.IntegrationTests.Scenario_01
{
    /// <summary>
    /// An instance of this class is shared across all the test method of <see cref="T01_MeasurementUnits"/>
    /// </summary>
    public class Scenario_01_WebApplicationFactory : WebApplicationFactory<Startup>
    {
        private ServiceProvider _provider;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Common mistake from the developer, catch it early
            ThrowIfDuplicates(Translation.TRANSLATIONS);

            builder.ConfigureServices(services =>
            {
                // Keep all the configuration the same except for the manager DB connection string
                using (var scope = services.BuildServiceProvider().CreateScope())
                {
                    var config = new CustomConnectionStringConfiguration(
                        scope.ServiceProvider.GetRequiredService<IConfiguration>(), adminDbName: "BSharp-Scenario-01");

                    services.AddSingleton<IConfiguration>(config);
                    new Startup(config).ConfigureServices(services);
                }

                //////////// Setup
                _provider = services.BuildServiceProvider();
                using (var scope = _provider.CreateScope())
                {
                    // Note: The goal is to eventually trim this down to just provisioning the databases
                    // and have the remainder of the setup and configuratio done in the tests through the API, when the API is ready

                    // (1) Admin Context migrated the usual way, add one tenant for dev and all translations
                    var adminContext = scope.ServiceProvider.GetRequiredService<AdminContext>();
                    adminContext.Database.EnsureDeleted();
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

                    // Seed translations
                    adminContext.Database.ExecuteSqlCommand("DELETE FROM [dbo].[Translations]");

                    adminContext.Translations.AddRange(Translation.TRANSLATIONS);
                    adminContext.SaveChanges();


                    // (2) Application Context requires special handling in development, don't resolve it with DI
                    var shardResolver = scope.ServiceProvider.GetRequiredService<IShardResolver>();
                    var appContext = new ApplicationContext(shardResolver,
                        new DesignTimeTenantIdProvider(), new DesignTimeUserIdProvider());

                    appContext.Database.Migrate();
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            //////////// Cleanup
            using (var scope = _provider.CreateScope())
            {
                var adminContext = scope.ServiceProvider.GetRequiredService<AdminContext>();
                adminContext.Database.EnsureDeleted();
            }
        }

        private HttpClient _client;
        public HttpClient GetClient()
        {
            if (_client == null)
            {
                _client = CreateClient();
                _client.DefaultRequestHeaders.Add("Tenant-Id", "101");
            }

            return _client;
        }

        private SharedCollection _shared;
        public SharedCollection GetSharedCollection()
        {
            if(_shared == null)
            {
                _shared = new SharedCollection();
            }

            return _shared;
        }

        private static void ThrowIfDuplicates(Translation[] translations)
        {

            var duplicates = translations.GroupBy(e => (e.CultureId, e.Name)).Where(g => g.Count() > 1);
            if (duplicates.Any())
            {
                string str = string.Join(", ", duplicates.Select(g => g.Key));
                throw new InvalidOperationException($"Duplicate Translation Keys: {str}");
            }
        }
    }
}
