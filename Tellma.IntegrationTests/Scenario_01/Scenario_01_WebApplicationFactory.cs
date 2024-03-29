﻿using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Tellma.Services.EmbeddedIdentityServer;
using M = IdentityServer4.Models;

namespace Tellma.IntegrationTests.Scenario_01
{
    /// <summary>
    /// An instance of this class is shared across all test classes inherting from <see cref="Scenario_01"/>
    /// </summary>
    public class Scenario_01_WebApplicationFactory : WebApplicationFactory<Startup>
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private HttpClient _client;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // This instructs the web host to use the appsettings.json file in the
            // test project not the one in the original project being tested
            var projectDir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectDir, "appsettings.tests.json");
            builder.ConfigureAppConfiguration((_, cfg) =>
            {
                cfg.AddJsonFile(configPath);
                cfg.AddUserSecrets(typeof(Scenario_01).Assembly);
            });

            builder.ConfigureTestServices(services =>
            {
                // This reconfigures identity server to accept the ResourceOwnerPassword
                // grant which enables the integration tests to acquire access tokens
                // directly using the username and password
                services.AddSingleton<IUserClientsProvider, IntegrationTestsClientsProvider>();
            });

            //// This initializes the test database
            //builder.ConfigureServices(services =>
            //{
            //    var sp = services.BuildServiceProvider();

            //    // This won't run automatically when using WebApplicationFactory
            //    Program.InitDatabase(sp).Wait();

            //    // Databases seeding and preparation
            //    string connString;
            //    string adminEmail;
            //    IShardResolver shardResolver;

            //    using (var scope = sp.CreateScope())
            //    {
            //        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            //        connString = config.GetConnectionString(Constants.AdminConnection);
            //        adminEmail = config.Get<GlobalOptions>()?.Admin?.Email ?? "admin@tellma.com";
            //        shardResolver = scope.ServiceProvider.GetRequiredService<IShardResolver>();

            //        // Retrieve the access token specific to the developer
            //        _token = config.GetValue<string>("AccessToken");
            //    }

            //    ArrangeDatabasesForTests(connString, adminEmail, shardResolver);
            //});
        }

        //private void ArrangeDatabasesForTests(string adminConnString, string adminEmail, IShardResolver shardResolver)
        //{
        //    // Prepare the Admin database
        //    var databaseId = 101; // from SqlDatabase.Id IDENTITY(101, 1)
        //    var projectDir = Directory.GetCurrentDirectory();
        //    var seedAdminPath = Path.Combine(projectDir, "SeedAdmin.sql");
        //    var seedAdminSql = File.ReadAllText(seedAdminPath);

        //    using (var conn = new SqlConnection(adminConnString))
        //    {
        //        using (var cmd = conn.CreateCommand())
        //        {
        //            cmd.Parameters.AddWithValue("@Email", adminEmail);
        //            cmd.Parameters.AddWithValue("@DatabaseName", $"Tellma.IntegrationTests.{databaseId}");
        //            cmd.CommandText = seedAdminSql;

        //            conn.Open();
        //            cmd.ExecuteNonQuery();
        //        }
        //    }

        //    var appConnString = shardResolver.GetConnectionString(databaseId, cancellation: default).GetAwaiter().GetResult();
        //    var seedAppPath = Path.Combine(projectDir, "SeedApplication.sql");
        //    var seedAppSql = File.ReadAllText(seedAppPath);

        //    using (var conn = new SqlConnection(appConnString))
        //    {
        //        using (var cmd = conn.CreateCommand())
        //        {
        //            cmd.Parameters.AddWithValue("@Email", adminEmail);
        //            cmd.CommandText = seedAppSql;

        //            conn.Open();
        //            cmd.ExecuteNonQuery();
        //        }
        //    }
        //}

        protected override void ConfigureClient(HttpClient client)
        {
            //client.DefaultRequestHeaders.Add("X-Tenant-Id", "101");

            //// This extremely long-lived access token (life time of 6 years) was specifically generated for the integration tests
            //client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");

            //// Version headers
            //client.DefaultRequestHeaders.Add("x-definitions-version", $"???");
            //client.DefaultRequestHeaders.Add("x-global-settings-version", $"???");
            //client.DefaultRequestHeaders.Add("x-permissions-version", $"???");
            //client.DefaultRequestHeaders.Add("x-settings-version", $"???");
            //client.DefaultRequestHeaders.Add("x-user-settings-version", $"???");
        }

        public HttpClient GetClient()
        {
            // Client is created once and disposed in Dispose()
            if (_client == null)
            {
                _client = CreateClient();

                // TODO: Additional configuration

                _disposables.Add(_client);
            }

            return _client;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

            _disposables.Clear();
        }

        #region Mocks

        /// <summary>
        /// A fake implementation that allows integration tests to use the resource
        /// owner password grant instead of the implicit grant to authenticate users.
        /// </summary>
        private class IntegrationTestsClientsProvider : IUserClientsProvider
        {
            public IEnumerable<M.Client> UserClients()
            {
                yield return new M.Client
                {
                    ClientId = Tellma.Services.Utilities.Constants.WebClientName,
                    ClientSecrets =
                        {
                            new Secret("top-secret".Sha256())
                        },
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowedScopes =
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            IdentityServerConstants.StandardScopes.Email,
                            Tellma.Services.Utilities.Constants.ApiResourceName
                        },
                };
            }
        }

        #endregion
    }
}


