using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BSharp.Data.Model
{
    public class MeasurementUnit : ModelBase, IAuditedModel
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

        [Required]
        [MaxLength(450)]
        public string CreatedBy { get; set; }

        public DateTimeOffset ModifiedAt { get; set; }

        [Required]
        [MaxLength(450)]
        public string ModifiedBy { get; set; }


        internal static void OnModelCreating(ModelBuilder builder)
        {
            // IsActive defaults to TRUE
            builder.Entity<MeasurementUnit>().Property(e => e.IsActive).HasDefaultValue(true);

            // Code is unique
            builder.Entity<MeasurementUnit>().HasIndex("TenantId", nameof(Code)).IsUnique();
        }
    }
}
