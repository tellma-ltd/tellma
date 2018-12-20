using BSharp.Data.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

// dotnet ef migrations add <MigrationName> -c=AdminContext -o=Data/Migrations/Admin
namespace BSharp.Data
{
    /// <summary>
    /// The admin context contains all tables that do not have TenantId, and therefore
    /// cannot be distributed across shardse, this so far includes:
    ///  - Shard Manager
    ///  - Localization
    ///  - Application-wide configuration
    ///  - Tenant User-memberships (This is only for convenince as the same info is also in the shards)
    /// The above functions can be split into their own databases later, but are kept together now for 
    /// developer convenience, the admin context implements the embedded identity framework of the 
    /// application, it derived from IdentityUserContext to avoid adding unnecessary Roles table.
    /// </summary>
    public class AdminContext : DbContext
    {
        public AdminContext(DbContextOptions<AdminContext> opt) : base(opt) { }

        // Shard Manager
        public DbSet<Shard> Shards { get; set; }
        public DbSet<Tenant> Tenants { get; set; }

        // Localization
        public DbSet<Culture> Cultures { get; set; }
        public DbSet<Translation> Translations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // All the model definitions has been moved to the
            // model files themselves for better code readability
            Culture.OnModelCreating(builder);
            Translation.OnModelCreating(builder);
            Tenant.OnModelCreating(builder);
            Shard.OnModelCreating(builder);
        }
    }
}
