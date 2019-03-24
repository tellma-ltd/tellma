using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BSharp.Data.DbModel
{
    public class MeasurementUnit : DbModelBase, IAuditedModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Name2 { get; set; }

        [MaxLength(255)]
        public string Code { get; set; }

        [Required]
        [MaxLength(255)]
        public string UnitType { get; set; }

        public double UnitAmount { get; set; }

        public double BaseAmount { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public int CreatedById { get; set; }
        public LocalUser CreatedBy { get; set; }

        public DateTimeOffset ModifiedAt { get; set; }

        public int ModifiedById { get; set; }
        public LocalUser ModifiedBy { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            // IsActive defaults to TRUE
            builder.Entity<MeasurementUnit>().Property(e => e.IsActive).HasDefaultValue(true);

            // Code is unique
            builder.Entity<MeasurementUnit>().HasIndex(TenantId, nameof(Code)).IsUnique();

            // Audit foreign keys
            builder.Entity<MeasurementUnit>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(TenantId, nameof(CreatedById))
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MeasurementUnit>()
                .HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(TenantId, nameof(ModifiedById))
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
