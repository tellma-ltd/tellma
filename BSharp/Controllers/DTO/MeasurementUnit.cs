using BSharp.Controllers.Misc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Controllers.DTO
{
    /// <summary>
    /// All savable DTOs must inherit from <see cref="DtoForSaveKeyBase{TKey}"/>
    /// </summary>
    [CollectionName("MeasurementUnits")]
    public class MeasurementUnitForSave : DtoForSaveKeyBase<int?>
    {
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Name2")]
        public string Name2 { get; set; }

        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [ChoiceList(new object[] { "Pure", "Time", "Distance", "Count", "Mass", "Volume", "Money" },
            new string[] { "MU_Pure", "MU_Time", "MU_Distance", "MU_Count", "MU_Mass", "MU_Volume", "MU_Money" })]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "MU_UnitType")]
        public string UnitType { get; set; }

        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [Display(Name = "MU_UnitAmount")]
        public double? UnitAmount { get; set; }

        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [Display(Name = "MU_BaseAmount")]
        public double? BaseAmount { get; set; }
    }

    /// <summary>
    /// The read-DTO, which always inherits from the update-DTO
    /// </summary>
    public class MeasurementUnit : MeasurementUnitForSave, IAuditedDto
    {
        [Display(Name = "IsActive")]
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
