using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Data.Model
{
    // This table is to specify which views are active and which aren't
    public class View: ModelBase
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
