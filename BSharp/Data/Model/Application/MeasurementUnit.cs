using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data.Model.Application
{
    public class MeasurementUnit : ModelForSaveBase, IAuditedModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name1 { get; set; }

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
        public string CreatedBy { get; set; }

        public DateTimeOffset ModifiedAt { get; set; }

        [Required]
        public string ModifiedBy { get; set; }
    }
}
