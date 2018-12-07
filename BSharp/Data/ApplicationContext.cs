using BSharp.Data.Model;
using BSharp.Services.Identity;
using BSharp.Services.Migrations;
using BSharp.Services.MultiTenancy;
using BSharp.Services.Sharding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

// dotnet ef migrations add Initial -c=ApplicationContext -o=Data/Migrations/Application
namespace BSharp.Data
{
    public class ApplicationContext : DbContext
    {
        // The database tables are listed below
        public DbSet<Translation> Translations { get; set; }
        public DbSet<MeasurementUnit> MeasurementUnits { get; set; }

        /// <summary>
        /// A query for returning strongly typed validation errors from SQL
        /// </summary>
        public DbQuery<SqlValidationResult> Validation { get; set; }

        /// <summary>
        /// A query for returning strongly typed Id mappings from SQL
        /// </summary>
        public DbQuery<IndexedId> Saving { get; set; }

        // Private fields
        private readonly ITenantIdProvider _tenantIdProvider;

        // Constructor
        public ApplicationContext(IShardResolver shardProvider, ITenantIdProvider tenantIdProvider, IUserIdProvider userIdProvider) :
            base(CreateDbContextOptions(shardProvider, tenantIdProvider, userIdProvider))
        {
            _tenantIdProvider = tenantIdProvider;
        }

        /// <summary>
        /// This trick makes it possible to injected the ApplicationContext into other components via DI as usual
        /// but it automatically configures itself with the correct options. Inspired from this Microsoft sample: https://bit.ly/2TIEFMA
        /// </summary>
        /// <param name="shardResolver">The service that resolves the shard connection string</param>
        /// <param name="tenantIdProvider">The service that retrieves tenants Ids from the headers</param>
        /// <returns></returns>
        private static DbContextOptions<ApplicationContext> CreateDbContextOptions(
            IShardResolver shardResolver, ITenantIdProvider tenantIdProvider, IUserIdProvider userIdProvider)
        {
            // Prepare the options based on the connection created with the shard manager
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            string connectionString = shardResolver.GetShardConnectionString();
            if (tenantIdProvider is DesignTimeTenantIdProvider)
            {
                // Only for design time when running "ef migrations" command from the CLI
                optionsBuilder = optionsBuilder.UseSqlServer(connectionString);
            }
            else
            {
                // Unless this is a fake design time resolver, apply row level security and pass context info
                SqlConnection sqlConnection = new SqlConnection(connectionString);

                SqlCommand cmd = sqlConnection.CreateCommand();
                cmd.CommandText = @"
EXEC sp_set_session_context @key=N'TenantId', @value=@TenantId;
EXEC sp_set_session_context @key=N'UserId', @value=@UserId;
EXEC sp_set_session_context @key=N'Culture', @value=@Culture;
EXEC sp_set_session_context @key=N'NeutralCulture', @value=@NeutralCulture;
";
                cmd.Parameters.AddWithValue("@TenantId", tenantIdProvider.GetTenantId() ?? throw new InvalidOperationException("Tenant Id was not supplied"));
                cmd.Parameters.AddWithValue("@UserId", userIdProvider.GetUserId());
                cmd.Parameters.AddWithValue("@Culture", CultureInfo.CurrentCulture.Name);
                cmd.Parameters.AddWithValue("@NeutralCulture", CultureInfo.CurrentCulture.IsNeutralCulture ? CultureInfo.CurrentCulture.Name : CultureInfo.CurrentCulture.Parent.Name);

                sqlConnection.Open(); // It gets disposed automatically when the DbContext is disposed
                cmd.ExecuteNonQuery();

                // Prepare the options based on the connection created with the shard manager
                optionsBuilder = optionsBuilder.UseSqlServer(sqlConnection);
            }

            return optionsBuilder
                .ReplaceService<IMigrationsSqlGenerator, CustomSqlServerMigrationsSqlGenerator>()
                .Options;
        }

        /// <summary>
        /// This method configures the database model with EF Core's fluent API
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Translations
            AddTenantId<Translation>(builder, nameof(Translation.Culture), nameof(Translation.Name));

            // Measurement Units
            AddTenantId<MeasurementUnit>(builder);
            MeasurementUnit.OnModelCreating(builder);
        }

        /// <summary>
        /// Adds a shadow property "TenantId" to the entity collection, adds that property to the entity keys, 
        /// and adds a query filter based on the tenantIdProvider
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="keyPropertyNames">The properties to include in the primary key, it uses one property "Id" by default if none are specified</param>
        private void AddTenantId<T>(ModelBuilder builder, params string[] keyPropertyNames) where T : class
        {
            string tenantId = "TenantId";

            // Add the TenantId shadow property
            builder.Entity<T>().Property<int>(tenantId);

            // Add the TenantId query filter (TODO: Remove)
            builder.Entity<T>().HasQueryFilter(e => EF.Property<int?>(e, tenantId) == _tenantIdProvider.GetTenantId());

            // Make TenantId part of the composite primary key
            List<string> keys = new List<string>(keyPropertyNames);
            if (keys.Count == 0)
            {
                keys.Add("Id");
                builder.Entity<T>()
                    .Property("Id")
                    .ValueGeneratedOnAdd();
            }

            keys = keys.Prepend(tenantId).ToList();
            builder.Entity<T>().HasKey(keys.ToArray());
        }

        #region Design Time Factory

        /// <summary>
        /// Since <see cref="ApplicationContext"/> does not have the usual DbContext constructor and is not 
        /// registered in the DI container the standard way, this factory implementation is necessary for the 
        /// migration tools to be able to create an instance of the context at design time
        /// </summary>
        public class DesignTimeApplicationContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
        {
            public ApplicationContext CreateDbContext(string[] args)
            {
                return new ApplicationContext(
                    new DesignTimeShardResolver(),
                    new DesignTimeTenantIdProvider(),
                    new DesignTimeUserIdProvider()
                );
            }
        }

        public class DesignTimeShardResolver : IShardResolver
        {
            public string GetShardConnectionString()
            {
                return "<FakeConnectionString>";
            }
        }

        public class DesignTimeTenantIdProvider : ITenantIdProvider
        {
            public int? GetTenantId()
            {
                return -1;
            }
        }

        public class DesignTimeUserIdProvider : IUserIdProvider
        {
            public string GetUserId()
            {
                return "<FakeUserId>";
            }
        }

        #endregion
    }
}
