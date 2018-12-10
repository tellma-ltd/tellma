using System;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Controllers.DTO
{
    /// <summary>
    /// All savable DTOs must inherit from <see cref="DtoForSaveKeyBase{TKey}"/>
    /// </summary>
    public class MeasurementUnitForSave : DtoForSaveKeyBase<int>
    {
        [Required]
        [StringLength(255)]
        [Display(Name = "MeasurementUnit.Name1")]
        public string Name1 { get; set; }

        [StringLength(255)]
        [Display(Name = "MeasurementUnit.Name2")]
        public string Name2 { get; set; }

        [StringLength(255)]
        [Display(Name = "MeasurementUnit.Code")]
        public string Code { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "MeasurementUnit.UnitType")]
        public string UnitType { get; set; }

        [Required]
        [Display(Name = "MeasurementUnit.UnitAmount")]
        public double? UnitAmount { get; set; }

        [Required]
        [Display(Name = "MeasurementUnit.BaseAmount")]
        public double? BaseAmount { get; set; }
    }

    /// <summary>
    /// The read-DTO, which always inherits from the update-DTO
    /// </summary>
    public class MeasurementUnit : MeasurementUnitForSave, IAuditedDto
    {
        [Display(Name = "MeasurementUnit.IsActive")]
        public bool? IsActive { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public string CreatedBy { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public string ModifiedBy { get; set; }
    }
}
