using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BSharp.Data.Model
{
    /// <summary>
    /// A record specifies that tenant X lives in Database Y
    /// </summary>
    public class Tenant
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Name2 { get; set; }

        [MaxLength(255)]
        public string Code { get; set; }

        public int ShardId { get; set; }
        public Shard Shard { get; set; }

        public ICollection<TenantMembership> Members { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            // Switch off auto-increment for Id
            builder.Entity<Tenant>()
                .Property(e => e.Id)
                .ValueGeneratedNever();
        }
    }
}
