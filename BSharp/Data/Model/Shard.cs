using System;
using System.ComponentModel.DataAnnotations;
using BSharp.Services.Sharding;
using Microsoft.EntityFrameworkCore;

namespace BSharp.Data.Model
{
    public class Shard
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string ConnectionString { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            // There is always a minium of one shard that resides in the manager context itself
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
