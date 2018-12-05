using BSharp.Controllers.Shared;
using System;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Controllers.Application.DTO
{
    /// <summary>
    /// All savable DTOs must inherit from <see cref="DtoForSaveKeyBase{TKey}"/>
    /// </summary>
    public class MeasurementUnitForSave : DtoForSaveKeyBase<int>
    {
        [Required]
        [StringLength(255)]
        [Display(Name = "Name")] // TODO show a localized name
        public string Name1 { get; set; }

        [StringLength(255)]
        [Display(Name = "Name")] // TODO show a localized name
        public string Name2 { get; set; }

        [StringLength(255)]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Unit Type")]
        public string UnitType { get; set; }

        [Required]
        [Display(Name = "Unit Amount")]
        public double? UnitAmount { get; set; }

        [Required]
        [Display(Name = "Base Amount")]
        public double? BaseAmount { get; set; }
    }

    /// <summary>
    /// The read-DTO, which always inherits from the update-DTO
    /// </summary>
    public class MeasurementUnit : MeasurementUnitForSave, IAuditedDto
    {
        public bool? IsActive { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public DateTimeOffset? ModifiedAt { get; set; }

        public string ModifiedBy { get; set; }
    }
}
