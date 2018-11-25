using BSharp.Data.Model.Sharding;
using BSharp.Services.Sharding;
using Microsoft.EntityFrameworkCore;

// dotnet ef migrations add <MigrationName> -c=ShardingContext -o=Data/Migrations/Sharding
namespace BSharp.Data
{
    public class ShardingContext : DbContext
    {
        public ShardingContext(DbContextOptions<ShardingContext> opt) : base(opt) { }

        public DbSet<Shard> Shards { get; set; }
        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Switch off auto-increment for Id
            builder.Entity<Tenant>()
                .Property(e => e.Id)
                .ValueGeneratedNever();

            // There is always a minium of one shard that lives in the Shard manager itself
            builder.Entity<Shard>()
                .HasData(new Shard
                {
                    Id = 1,
                    Name = "Shard Manager",
                    ConnectionString = ShardResolver.SHARD_MANAGER_PLACEHOLDER
                });
        }
    }
}
