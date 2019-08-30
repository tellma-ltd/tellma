using BSharp.IntegrationTests.Utilities;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;

namespace BSharp.IntegrationTests.Scenario_01
{
    /// <summary>
    /// An instance of this class is shared across all the test method of <see cref="T01_MeasurementUnits"/>
    /// </summary>
    public class Scenario_01_WebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // This instructs the web host to use the appsettings.json file in the
            // test project not the one in the original project being tested
            var projectDir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectDir, "appsettings.tests.json");
            builder.ConfigureAppConfiguration((_, cfg) =>
            {
                cfg.AddJsonFile(configPath);
            });

            // Here we do database seeding and arranging
            builder.ConfigureServices(services =>
            {
                var provider = services.BuildServiceProvider();
                Program.InitDatabase(provider); // This won't run automatically when using WebApplicationFactory

                using(var scope = provider.CreateScope())
                {
                    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var connString = config.GetConnectionString(Constants.AdminConnection);

                    ArrangeDatabaseForTests(connString);
                }
            });
        }

        private void ArrangeDatabaseForTests(string connString)
        {
            var projectDir = Directory.GetCurrentDirectory();
            var seedAdminPath = Path.Combine(projectDir, "SeedAdmin.sql");
            var seedAdminSql = System.IO.File.ReadAllText(seedAdminPath);

            using (var conn = new SqlConnection(connString))
            {
                using(var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = seedAdminSql;
                    conn.Open();

                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if(_client != null)
            {
                _client.Dispose();
            }
        }

        private HttpClient _client;
        public HttpClient GetClient()
        {
            if (_client == null)
            {
                _client = CreateClient();
                _client.DefaultRequestHeaders.Add("X-Tenant-Id", "101");

                // This extremely long-lived access token (life time of 6 years) was specifically generated for the integration tests
                _client.DefaultRequestHeaders.Add("Authorization", "Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjJiOGY5ZmU3NzQ3ZTA3YzA2NzlkNjMzYzg4ZDM3MmMxIiwidHlwIjoiSldUIn0.eyJuYmYiOjE1NjcxNzgyMjQsImV4cCI6MTgwOTA5ODIyNCwiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NDQzNjgiLCJhdWQiOlsiaHR0cHM6Ly9sb2NhbGhvc3Q6NDQzNjgvcmVzb3VyY2VzIiwiYnNoYXJwIl0sImNsaWVudF9pZCI6IldlYkNsaWVudCIsInN1YiI6ImFlNDcyYTUwLWEyYzAtNGE1ZC04ZjI3LTk2ZDhiMTk4MTkzMyIsImF1dGhfdGltZSI6MTU2NzA4MzkyMywiaWRwIjoibG9jYWwiLCJlbWFpbCI6ImFkbWluQGJzaGFycC5vbmxpbmUiLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwic2NvcGUiOlsib3BlbmlkIiwicHJvZmlsZSIsImVtYWlsIiwiYnNoYXJwIl0sImFtciI6WyJwd2QiXX0.jUiXEZe36NBoWzwVVkLyM_FPgHqAmxiotPZbGZqr9nFxAwERiQ0qc8iSwUhZZon73Iq9ITL9gDijDGF4txvtopgPlpbn94d5FycjlZKD4azgXHtdIfwWAK0N0qRkZD0W9-Wxcdl-sZJjAlbYSeWCRAcx2i-_3Je_79dRf3wQvqgX4v8Wti6snt85Blgz2kazJ80o9NLpFxBwliU09MXqpH6PblcSUMd3EaO7GTw7LFt5eoB_MucqDg-8puUzYETC-9oy14XDKqeT7LmyNBwy3GI70rCHKknEJSmmsAY1QcdpxXAJtiNcHrZfHK3FYQf7Fjb51w9AMXNHrmKama1Z4g");
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
    }
}


//            builder.ConfigureServices(services =>
//            {
//                //////////// Setup
//                _provider = services.BuildServiceProvider();
//                using (var scope = _provider.CreateScope())
//                {
//                    // Note: The goal is to eventually trim this down to just provisioning the databases
//                    // and have the remainder of the setup and configuratio done in the tests through the API, when the API is ready

//                    // (1) Admin Context migrated the usual way, add one tenant for dev and all translations
//                    var repo = scope.ServiceProvider.GetRequiredService<AdminRepository>();
//                    repo.Database.EnsureDeleted();
//                    repo.Database.Migrate();
//                    if (!repo.Tenants.Any())
//                    {
//                        repo.Tenants.Add(new Tenant
//                        {
//                            Id = 101,
//                            Name = "Contoso, Inc.",
//                            ShardId = 1
//                        });

//                        repo.Tenants.Add(new Tenant
//                        {
//                            Id = 102,
//                            Name = "Fabrikam & Co.",
//                            ShardId = 1
//                        });

//                        repo.SaveChanges();
//                    }

//                    repo.GlobalUsers.Add(new GlobalUser
//                    {
//                        Email = "support@banan-it.com",
//                        ExternalId = "4F7785F2-5942-4CFB-B5AD-85AB72F7EB35",
//                        Memberships = new List<TenantMembership> {
//                            new TenantMembership { TenantId = 101 },
//                            new TenantMembership { TenantId = 102 }
//                        }
//                    });

//                    repo.SaveChanges();

//                    // (2) Application Context requires special handling in development, don't resolve it with DI
//                    var shardResolver = scope.ServiceProvider.GetRequiredService<IShardResolver>();
//                    using (var appContext = new ApplicationContext(shardResolver,
//                        new DesignTimeTenantIdProvider(),
//                        new DesignTimeUserIdProvider(),
//                        new DesignTimeTenantUserInfoAccessor()))
//                    {

//                        appContext.Database.Migrate();

//                        // Add first user
//                        var now = DateTimeOffset.Now;
//                        appContext.Database.ExecuteSqlCommand(
//                            @"
//DECLARE @NextId INT = IDENT_CURRENT('[dbo].[LocalUsers]') + 1;
//INSERT INTO [dbo].[LocalUsers] (Email, ExternalId, CreatedAt, ModifiedAt, Name, Name2, CreatedById, ModifiedById, TenantId)
//                            VALUES ({0}, {1}, {2}, {2}, {3}, {4}, @NextId, @NextId, 101)",

//                            "support@banan-it.com", // {0}
//                            "4F7785F2-5942-4CFB-B5AD-85AB72F7EB35", // {1}
//                            now, // {2}
//                            "Banan IT Support", // {3}
//                            "فريق مساندة بنان"); // {4}

//                        // The security administrator role
//                        int userId = 1;
//                        var saRole = new Role
//                        {
//                            Name = "Security Administrator",
//                            Name2 = "مدير الأمان",
//                            Code = "SA",
//                            IsActive = true,
//                            CreatedById = userId,
//                            ModifiedById = userId,
//                            CreatedAt = now,
//                            ModifiedAt = now,
//                            Permissions = new List<Permission>
//                            {
//                                new Permission {
//                                    ViewId = "local-users",
//                                    Action = "Update",
//                                    Criteria = "Id lt 100000",
//                                    CreatedById = userId,
//                                    ModifiedById = userId,
//                                    CreatedAt = now,
//                                    ModifiedAt = now,
//                                },
//                                new Permission {
//                                    ViewId = "roles",
//                                    Action = "Update",
//                                    Criteria = "Id lt 100000",
//                                    CreatedById = userId,
//                                    ModifiedById = userId,
//                                    CreatedAt = now,
//                                    ModifiedAt = now,
//                                },
//                                new Permission {
//                                    ViewId = "views",
//                                    Action = "Read",
//                                    Criteria = null,
//                                    CreatedById = userId,
//                                    ModifiedById = userId,
//                                    CreatedAt = now,
//                                    ModifiedAt = now,
//                                }
//                            },
//                            Members = new List<RoleMembership>
//                            {
//                                new RoleMembership {
//                                    UserId = 1,
//                                    CreatedById = userId,
//                                    ModifiedById = userId,
//                                    CreatedAt = now,
//                                    ModifiedAt = now,
//                                }
//                            }
//                        };

//                        appContext.Roles.Add(saRole);

//                        appContext.Entry(saRole).Property("TenantId").CurrentValue = 101;
//                        appContext.Entry(saRole.Permissions.First()).Property("TenantId").CurrentValue = 101;
//                        appContext.Entry(saRole.Permissions.Last()).Property("TenantId").CurrentValue = 101;
//                        appContext.Entry(saRole.Members.Last()).Property("TenantId").CurrentValue = 101;

//                        // Add the views
//                        appContext.Views.Add(new View { Id = "local-users", IsActive = true });
//                        appContext.Views.Add(new View { Id = "roles", IsActive = true });
//                        appContext.Views.Add(new View { Id = "measurement-units", IsActive = true });
//                        appContext.Views.Add(new View { Id = "individuals", IsActive = true });
//                        appContext.Views.Add(new View { Id = "organizations", IsActive = true });
//                        appContext.Views.Add(new View { Id = "views", IsActive = true });
//                        appContext.Views.Add(new View { Id = "settings", IsActive = true });
//                        appContext.Views.Add(new View { Id = "ifrs-notes", IsActive = true });
//                        appContext.Views.Add(new View { Id = "product-categories", IsActive = true });

//                        // Add the settings
//                        var settings = new Settings
//                        {
//                            PrimaryLanguageId = "en",
//                            ProvisionedAt = now,
//                            ModifiedAt = now,
//                            ModifiedById = userId,
//                            ShortCompanyName = "Contoso, Inc."
//                        };
//                        appContext.Settings.Add(settings);
//                        appContext.Entry(settings).Property("TenantId").CurrentValue = 101;

//                        // Save all of the above
//                        appContext.SaveChanges();
//                    }
//                }
//            });
