using BSharp.Data;
using BSharp.Data.Model;
using BSharp.IntegrationTests.Utilities;
using BSharp.Services.Sharding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                    adminContext.GlobalUsers.Add(new GlobalUser
                    {
                        Email = "support@banan-it.com",
                        ExternalId = "4F7785F2-5942-4CFB-B5AD-85AB72F7EB35",
                        Memberships = new List<TenantMembership> {
                            new TenantMembership { TenantId = 101 },
                            new TenantMembership { TenantId = 102 }
                        }
                    });

                    adminContext.SaveChanges();

                    // (2) Application Context requires special handling in development, don't resolve it with DI
                    var shardResolver = scope.ServiceProvider.GetRequiredService<IShardResolver>();
                    using (var appContext = new ApplicationContext(shardResolver,
                        new DesignTimeTenantIdProvider(), new DesignTimeUserIdProvider()))
                    {

                        appContext.Database.Migrate();

                        // Add first user
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

                        // The security administrator role
                        int userId = 1;
                        var saRole = new Role
                        {
                            Name = "Security Administrator",
                            Name2 = "مدير الأمان",
                            Code = "SA",
                            IsActive = true,
                            CreatedById = userId,
                            ModifiedById = userId,
                            CreatedAt = now,
                            ModifiedAt = now,
                            Permissions = new List<Permission>
                            {
                                new Permission {
                                    ViewId = "local-users",
                                    Level = "Update",
                                    CreatedById = userId,
                                    ModifiedById = userId,
                                    CreatedAt = now,
                                    ModifiedAt = now,
                                },
                                new Permission {
                                    ViewId = "roles",
                                    Level = "Update",
                                    CreatedById = userId,
                                    ModifiedById = userId,
                                    CreatedAt = now,
                                    ModifiedAt = now,
                                }
                            },
                                Members = new List<RoleMembership>
                            {
                                new RoleMembership {
                                    UserId = 1,
                                    CreatedById = userId,
                                    ModifiedById = userId,
                                    CreatedAt = now,
                                    ModifiedAt = now,
                                }
                            }
                        };

                        appContext.Roles.Add(saRole);

                        appContext.Entry(saRole).Property("TenantId").CurrentValue = 101;
                        appContext.Entry(saRole.Permissions.First()).Property("TenantId").CurrentValue = 101;
                        appContext.Entry(saRole.Permissions.Last()).Property("TenantId").CurrentValue = 101;
                        appContext.Entry(saRole.Members.Last()).Property("TenantId").CurrentValue = 101;

                        // Add the views
                        appContext.Views.Add(new View { Id = "local-users", IsActive = true });
                        appContext.Views.Add(new View { Id = "roles", IsActive = true });
                        appContext.Views.Add(new View { Id = "measurement-units", IsActive = true });
                        appContext.Views.Add(new View { Id = "individuals", IsActive = true });
                        appContext.Views.Add(new View { Id = "organizations", IsActive = true });

                        appContext.SaveChanges();
                    }
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
            if (_shared == null)
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
