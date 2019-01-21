using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Data.Model
{
    public class Permission : ModelBase, IAuditedModel
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

        [Required]
        [MaxLength(450)]
        public string CreatedBy { get; set; }

        public DateTimeOffset ModifiedAt { get; set; }

        [Required]
        [MaxLength(450)]
        public string ModifiedBy { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            // Role foreign key
            builder.Entity<Permission>()
                .HasOne(e => e.Role)
                .WithMany(e => e.Permissions)
                .HasForeignKey("TenantId", nameof(RoleId))
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
