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
                    var env = scope.ServiceProvider.GetRequiredService<IHostingEnvironment>();
                    new Startup(config, env).ConfigureServices(services);
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

                    // Activate Arabic
                    adminContext.Database.ExecuteSqlCommand("UPDATE Cultures SET IsActive = 1 WHERE Id = 'ar'");

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
                        new DesignTimeTenantIdProvider(), 
                        new DesignTimeUserIdProvider(), 
                        new DesignTimeTenantUserInfoAccessor()))
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
                                    Criteria = "Id lt 100000",
                                    CreatedById = userId,
                                    ModifiedById = userId,
                                    CreatedAt = now,
                                    ModifiedAt = now,
                                },
                                new Permission {
                                    ViewId = "roles",
                                    Level = "Update",
                                    Criteria = "Id lt 100000",
                                    CreatedById = userId,
                                    ModifiedById = userId,
                                    CreatedAt = now,
                                    ModifiedAt = now,
                                },
                                new Permission {
                                    ViewId = "views",
                                    Level = "Read",
                                    Criteria = null,
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
                        appContext.Views.Add(new View { Id = "views", IsActive = true });
                        appContext.Views.Add(new View { Id = "settings", IsActive = true });

                        // Add the settings
                        var settings = new Settings
                        {
                            PrimaryLanguageId = "en",
                            ProvisionedAt = now,
                            ModifiedAt = now,
                            ModifiedById = userId,
                            ShortCompanyName = "Contoso, Inc."
                        };
                        appContext.Settings.Add(settings);
                        appContext.Entry(settings).Property("TenantId").CurrentValue = 101;

                        // Save all of the above
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
                _client.DefaultRequestHeaders.Add("X-Tenant-Id", "101");

                // This extremely long lived access token (life time of 6 years) was specifically generated for the integration tests
                _client.DefaultRequestHeaders.Add("Authorization", "Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjJiOGY5ZmU3NzQ3ZTA3YzA2NzlkNjMzYzg4ZDM3MmMxIiwidHlwIjoiSldUIn0.eyJuYmYiOjE1NTA2MTI2ODcsImV4cCI6MTczOTgyODY4NywiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NDQzMzkiLCJhdWQiOlsiaHR0cHM6Ly9sb2NhbGhvc3Q6NDQzMzkvcmVzb3VyY2VzIiwiYnNoYXJwIl0sImNsaWVudF9pZCI6IldlYkNsaWVudCIsInN1YiI6IjU1NTIyN2FiLWQ0N2MtNDY1OS1iMWJjLTYyOGIyODMzMGFlNCIsImF1dGhfdGltZSI6MTU1MDYxMjY4NSwiaWRwIjoibG9jYWwiLCJlbWFpbCI6InN1cHBvcnRAYmFuYW4taXQuY29tIiwiZW1haWxfdmVyaWZpZWQiOnRydWUsInNjb3BlIjpbIm9wZW5pZCIsInByb2ZpbGUiLCJlbWFpbCIsImJzaGFycCJdLCJhbXIiOlsicHdkIl19.po4g7c59T56X4WaqXkUUNmxsZp6t0Wu8dj_AfTG1bkLze_6XV-W_eKRKY6XUiZ9kKvwNzQ4dyQExbV_tN8I63NBWQnoqHHRB1Mw1_PDHTK-MrGzSIyLx40AtsI6-KJAyw8v74dr-71alx29Ccnvf59NJMP1uW-z-Ma945ePF5SvaY-BpRVWEhTuqO_fkS7DdxTWvPAt-cXTQ9zeREQfi9KC8eNZU6efqBXlueE5zxLKc458-aNa2PX7pmyyWJk_YupMVLzHBvu7KDT0e0M4JYkevFyRHr2742UX8G4SeHxT0-2VeYx5iA1dMMFrSKaF4c-vPNvJbccnU60b3zAdE5g");
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
