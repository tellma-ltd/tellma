using System;
using System.Collections.Generic;
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

        public string Name { get; set; }

        public int ShardId { get; set; }

        public Shard Shard { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            // Switch off auto-increment for Id
            builder.Entity<Tenant>()
                .Property(e => e.Id)
                .ValueGeneratedNever();
        }
    }
}
