using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data.Model
{
    // Any arbitrary key value pair that the UI wishes to save about the current user
    public class LocalUserSetting : ModelBase
    {
        public int UserId { get; set; }

        public LocalUser User { get; set; }

        [Required]
        [MaxLength(255)]
        public string Key { get; set; }

        [Required]
        [MaxLength(2048)]
        public string Value { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            // Add the TenantId shadow property
            builder.Entity<LocalUserSetting>().Property<int>(TenantId)
                .ValueGeneratedNever();

            // Default
            builder.Entity<LocalUserSetting>().Property(TenantId).HasDefaultValueSql("CONVERT(INT, SESSION_CONTEXT(N'TenantId'))");

            // Primary Key
            builder.Entity<LocalUserSetting>().HasKey(TenantId, nameof(UserId), nameof(Key));

            // Role foreign key
            builder.Entity<LocalUserSetting>()
                .HasOne(e => e.User)
                .WithMany(e => e.Settings)
                .HasForeignKey(TenantId, nameof(UserId))
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
