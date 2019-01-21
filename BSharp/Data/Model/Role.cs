using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Data.Model
{
    public class Role : ModelBase, IAuditedModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Name2 { get; set; }

        [MaxLength(255)]
        public string Code { get; set; }

        public bool IsPublic { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        [Required]
        [MaxLength(450)]
        public string CreatedBy { get; set; }

        public DateTimeOffset ModifiedAt { get; set; }

        [Required]
        [MaxLength(450)]
        public string ModifiedBy { get; set; }

        public ICollection<Permission> Permissions { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            // IsActive defaults to TRUE
            builder.Entity<Role>().Property(e => e.IsActive).HasDefaultValue(true);

            // Code is unique
            builder.Entity<Role>().HasIndex("TenantId", nameof(Code)).IsUnique();

            // Name is unique
            builder.Entity<Role>().HasIndex("TenantId", nameof(Name)).IsUnique();

            // Name2 is unique
            builder.Entity<Role>().HasIndex("TenantId", nameof(Name2)).IsUnique();
        }
    }
}
