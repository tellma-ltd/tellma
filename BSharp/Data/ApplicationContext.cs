using BSharp.Data.Model.Application;
using BSharp.Services.MultiTenancy;
using BSharp.Services.Sharding;
using BSharp.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// dotnet ef migrations add Initial -c=ApplicationContext -o=Data/Migrations/Application
namespace BSharp.Data
{
    public class ApplicationContext :DbContext
    {
        private readonly ITenantIdProvider _tenantIdProvider;

        public ApplicationContext(IShardResolver shardProvider, ITenantIdProvider tenantIdProvider) : 
            base(CreateOptions(shardProvider))
        {
            _tenantIdProvider = tenantIdProvider;
        }

        /// <summary>
        /// This trick makes it possible to injected the ApplicationContext into other components via DI as usual
        /// but it automatically configures itself with the correct options. Taken from this Microsoft sample: https://bit.ly/2TIEFMA
        /// </summary>
        /// <param name="shardResolver">The service that resolves the shard connection string</param>
        /// <param name="tenantIdProvider">The service that retrieves tenants Ids from the headers</param>
        /// <returns></returns>
        private static DbContextOptions<ApplicationContext> CreateOptions(IShardResolver shardResolver)
        {
            var connectionString = shardResolver.GetShardConnectionString();
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            var options = optionsBuilder.UseSqlServer(connectionString).Options;

            return options;
        }

        public DbSet<Translation> Translations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            AddTenantId<Translation>(builder);

            builder.Entity<Translation>()
                .HasKey("TenantId", nameof(Translation.Tier), nameof(Translation.Culture), nameof(Translation.Name));
        }

        /// <summary>
        /// Adds a shadow property "TenantId" to the entity collection, adds an index
        /// on that shadow property, and adds a query filter based on the tenantIdProvider
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        private void AddTenantId<T>(ModelBuilder builder) where T : class
        {
            string tenantId = "TenantId";

            builder.Entity<T>()
                .Property<int>(tenantId);

            builder.Entity<T>()
                .HasIndex(tenantId);

            builder.Entity<T>()
                .HasQueryFilter(e => EF.Property<int?>(e, tenantId) == _tenantIdProvider.GetTenantId());
        }

        #region Design Time Factory

        /// <summary>
        /// Since the ApplicationContext does not have the usual DbContext constructor and is not registered
        /// in the DI container the usual way, this factory implementation below is necessary for the migration
        /// tools to be able to create an instance of the context at design time
        /// </summary>
        public class DesignTimeApplicationContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
        {
            public ApplicationContext CreateDbContext(string[] args)
            {
                return new ApplicationContext(new DesignTimeShardResolver(), new DesignTimeTenantIdProvider());
            }

            private class DesignTimeShardResolver : IShardResolver
            {
                public string GetShardConnectionString()
                {
                    return "<FakeConnectionString>";
                }
            }

            private class DesignTimeTenantIdProvider : ITenantIdProvider
            {
                public int GetTenantId()
                {
                    return -1;
                }

                public bool IsTenantIdAvailable()
                {
                    return true;
                }
            }
        }

        #endregion
    }
}
