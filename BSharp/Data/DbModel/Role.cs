using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Data.DbModel
{
    public class Role : DbModelBase, IAuditedModel
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

        public ICollection<RoleMembership> Members { get; set; }

        public ICollection<Permission> Permissions { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public int CreatedById { get; set; }
        public LocalUser CreatedBy { get; set; }

        public DateTimeOffset ModifiedAt { get; set; }

        public int ModifiedById { get; set; }
        public LocalUser ModifiedBy { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            // IsActive defaults to TRUE
            builder.Entity<Role>().Property(e => e.IsActive).HasDefaultValue(true);

            // Code is unique
            builder.Entity<Role>().HasIndex(TenantId, nameof(Code)).IsUnique();

            // Name is unique
            builder.Entity<Role>().HasIndex(TenantId, nameof(Name)).IsUnique();

            // Name2 is unique
            builder.Entity<Role>().HasIndex(TenantId, nameof(Name2)).IsUnique();

            // For efficiently querying over IsPublic = true
            builder.Entity<Role>().HasIndex(TenantId, nameof(IsPublic)).HasFilter("[IsPublic] = 1");

            // Audit foreign keys
            builder.Entity<Role>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(TenantId, nameof(CreatedById))
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Role>()
                .HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(TenantId, nameof(ModifiedById))
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
