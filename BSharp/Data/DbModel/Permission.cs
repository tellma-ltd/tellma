using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Data.DbModel
{
    public class Permission : DbModelBase, IAuditedModel
    {
        public int Id { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }

        [Required]
        [MaxLength(255)]
        public string ViewId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Level { get; set; }

        [MaxLength(1024)]
        public string Criteria { get; set; }

        [MaxLength(255)]
        public string Memo { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public int CreatedById { get; set; }
        public LocalUser CreatedBy { get; set; }

        public DateTimeOffset ModifiedAt { get; set; }

        public int ModifiedById { get; set; }
        public LocalUser ModifiedBy { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            // Role foreign key
            builder.Entity<Permission>()
                .HasOne(e => e.Role)
                .WithMany(e => e.Permissions)
                .HasForeignKey(TenantId, nameof(RoleId))
                .OnDelete(DeleteBehavior.Cascade);

            // Audit foreign keys
            builder.Entity<Permission>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(TenantId, nameof(CreatedById))
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Permission>()
                .HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(TenantId, nameof(ModifiedById))
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
