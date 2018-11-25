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
            base(CreateDbOptions(shardProvider, tenantIdProvider))
        {
            _tenantIdProvider = tenantIdProvider;
        }

        /// <summary>
        /// This trick makes it possible to injected the ApplicationContext into other components via DI as usual
        /// but it automatically configures itself with the correct options. Taken from this Microsoft sample:
        /// https://github.com/Microsoft/WingtipTicketsSaaS-MultiTenantDB/blob/master/App/src/Events-TenantUserApp.EF/TenantsDB/TenantDbContext.cs
        /// </summary>
        /// <param name="shardResolver">The service that resolves the shard connection string</param>
        /// <param name="tenantIdProvider">The service that retrieves tenants Ids from the headers</param>
        /// <returns></returns>
        private static DbContextOptions<ApplicationContext> CreateDbOptions(IShardResolver shardResolver, ITenantIdProvider tenantIdProvider)
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
    }

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
                return "Fake Connection String";
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

}
