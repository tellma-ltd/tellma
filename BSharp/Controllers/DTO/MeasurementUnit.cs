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
    [StrongEntity]
    public class MeasurementUnitForSave : DtoForSaveKeyBase<int?>
    {
        [BasicField]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        public string Name { get; set; }

        [BasicField]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [MultilingualDisplay(Name = "Name", Language =  Language.Secondary)]
        public string Name2 { get; set; }

        [BasicField]
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
    public class MeasurementUnit : MeasurementUnitForSave
    {
        [BasicField]
        [Display(Name = "IsActive")]
        public bool? IsActive { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [ForeignKey]
        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [ForeignKey]
        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For Query

        [NavigationProperty(ForeignKey = nameof(CreatedById))]
        public LocalUser CreatedBy { get; set; }

        [NavigationProperty(ForeignKey = nameof(ModifiedById))]
        public LocalUser ModifiedBy { get; set; }
    }
}
