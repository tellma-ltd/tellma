using Microsoft.EntityFrameworkCore;
using System;

namespace BSharp.Data.Model
{
    public class RoleMembership : ModelBase, IAuditedModel
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public LocalUser User { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }

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
            builder.Entity<RoleMembership>()
                .HasOne(e => e.Role)
                .WithMany(e => e.Members)
                .HasForeignKey(TenantId, nameof(RoleId))
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RoleMembership>()
                .HasOne(e => e.User)
                .WithMany(e => e.Roles)
                .HasForeignKey(TenantId, nameof(UserId))
                .OnDelete(DeleteBehavior.Cascade);

            // Audit foreign keys
            builder.Entity<RoleMembership>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(TenantId, nameof(CreatedById))
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RoleMembership>()
                .HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(TenantId, nameof(ModifiedById))
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
