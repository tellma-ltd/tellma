using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BSharp.Data.DbModel
{
    public class Blob
    {
        public string Id { get; set; }

        [Required]
        public byte[] Content { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Blob>().HasIndex(e => e.Id).IsUnique();
        }
    }
}
