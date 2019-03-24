using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Data.DbModel
{
    // This table is to specify which views are active and which aren't
    public class View: DbModelBase
    {
        [Required]
        [MaxLength(255)]
        public string Id { get; set; }

        public bool IsActive { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            // IsActive defaults to TRUE
            builder.Entity<View>()
                .Property(e => e.IsActive)
                .HasDefaultValue(true);
        }
    }
}
