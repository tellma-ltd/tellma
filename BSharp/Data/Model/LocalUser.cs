using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data.Model
{
    public class LocalUser : ModelBase
    {
        public int Id { get; set; }
        
        [MaxLength(450)]
        public string ExternalId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Name2 { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Email { get; set; }

        public bool IsActive { get; set; }

        public int? AgentId { get; set; }
        public Agent Agent { get; set; }

        public ICollection<RoleMembership> Roles { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public int CreatedById { get; set; }
        public LocalUser CreatedBy { get; set; }

        public DateTimeOffset ModifiedAt { get; set; }

        public int ModifiedById { get; set; }
        public LocalUser ModifiedBy { get; set; }

        public DateTimeOffset? LastAccess { get; set; }

        /// <summary>
        /// Changes whenever the user or roles related to the user change
        /// </summary>
        public Guid PermissionsVersion { get; set; }

        /// <summary>
        /// Changes whenever the user settings change (to be implemented later)
        /// </summary>
        public Guid UserSettingsVersion { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            // IsActive defaults to TRUE
            builder.Entity<LocalUser>().Property(e => e.IsActive).HasDefaultValue(true);

            // ExternalId is unique
            builder.Entity<LocalUser>().HasIndex(TenantId, nameof(ExternalId)).IsUnique();

            // Email is unique
            builder.Entity<LocalUser>().HasIndex(TenantId, nameof(Email)).IsUnique();

            // Role foreign key
            builder.Entity<LocalUser>()
                .HasOne(e => e.Agent)
                .WithMany(e => e.Users)
                .HasForeignKey(TenantId, nameof(AgentId))
                .OnDelete(DeleteBehavior.Restrict);

            // Audit foreign keys
            builder.Entity<LocalUser>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(TenantId, nameof(CreatedById))
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LocalUser>()
                .HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(TenantId, nameof(ModifiedById))
                .OnDelete(DeleteBehavior.Restrict);

            // Just a random GUID
            Guid defaultGuid = new Guid("aafc6590-cadf-45fe-8c4a-045f4d6f73b1");
            builder.Entity<LocalUser>().Property(e => e.PermissionsVersion).HasDefaultValue(defaultGuid);
            builder.Entity<LocalUser>().Property(e => e.UserSettingsVersion).HasDefaultValue(defaultGuid);
        }
    }
}
