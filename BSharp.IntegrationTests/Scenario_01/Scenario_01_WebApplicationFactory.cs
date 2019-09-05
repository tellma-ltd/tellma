using BSharp.IntegrationTests.Utilities;
using BSharp.Services.Sharding;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;
using System.Reflection;

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
        }

        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            // Startup.ConfigureServices is called inside here
            var server = base.CreateServer(builder);

            // This won't run automatically when using WebApplicationFactory
            Program.InitDatabase(server.Host.Services);

            // Databases seeding and preparation
            string connString;
            string adminEmail;
            IShardResolver shardResolver;

            using (var scope = server.Host.Services.CreateScope())
            {
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                connString = config.GetConnectionString(Constants.AdminConnection);
                adminEmail = config.Get<GlobalOptions>()?.Admin?.Email ?? "admin@bsharp.online";
                shardResolver = scope.ServiceProvider.GetRequiredService<IShardResolver>();
            }

            ArrangeDatabasesForTests(connString, adminEmail, shardResolver);

            // Return the server
            return server;
        }

        private void ArrangeDatabasesForTests(string adminConnString, string adminEmail, IShardResolver shardResolver)
        {
            // Prepare the Admin database
            var databaseId = 101; // from SqlDatabase.Id IDENTITY(101, 1)
            var projectDir = Directory.GetCurrentDirectory();
            var seedAdminPath = Path.Combine(projectDir, "SeedAdmin.sql");
            var seedAdminSql = File.ReadAllText(seedAdminPath);

            using (var conn = new SqlConnection(adminConnString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@Email", adminEmail);
                    cmd.Parameters.AddWithValue("@DatabaseName", $"BSharp.IntegrationTests.{databaseId}");
                    cmd.CommandText = seedAdminSql;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            var appConnString = shardResolver.GetConnectionString(databaseId);
            var seedAppPath = Path.Combine(projectDir, "SeedApplication.sql");
            var seedAppSql = File.ReadAllText(seedAppPath);

            using (var conn = new SqlConnection(appConnString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@Email", adminEmail);
                    cmd.CommandText = seedAppSql;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected override void ConfigureClient(HttpClient client)
        {
            // TODO Configure the client here
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_client != null)
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


