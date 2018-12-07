using BSharp.Data.Model.Identity;
using BSharp.Data.Model.Localization;
using BSharp.Data.Model.Sharding;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

// dotnet ef migrations add <MigrationName> -c=ManagerContext -o=Data/Migrations/Manager
namespace BSharp.Data
{
    /// <summary>
    /// The manager context contains all tables that do not have TenantId, and therefore
    /// cannot be distributed across shards/database, this so far includes:
    ///  - Shard Manager
    ///  - Identity
    ///  - Core Localization
    ///  - Application-wide configuration
    ///  - Tenant User-memberships (This is only for convenince as the same info is also in the shards)
    /// The above functions can be split into their own databases later, but are kept together now for 
    /// developer convenience, the manager context implements the embedded identity framework of the 
    /// application, it derived from IdentityUserContext to avoid adding unnecessary Roles table.
    /// </summary>
    public class ManagerContext : IdentityUserContext<ApplicationUser>
    {
        public ManagerContext(DbContextOptions<ManagerContext> opt) : base(opt) { }

        // Shard Manager
        public DbSet<Shard> Shards { get; set; }
        public DbSet<Tenant> Tenants { get; set; }

        // Localization
        public DbSet<CoreTranslation> CoreTranslations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // All the model definitions has been moved to the
            // model files themselves for better code readability
            CoreTranslation.OnModelCreating(builder);
            Tenant.OnModelCreating(builder);
            Shard.OnModelCreating(builder);
        }
    }
}
