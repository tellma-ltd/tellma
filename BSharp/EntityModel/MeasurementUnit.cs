using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.EntityModel
{
    [StrongEntity]
    public class MeasurementUnitForSave : EntityKeyBase<int>
    {
        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Name { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Name2 { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Name3 { get; set; }

        [Display(Name = "Code")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Code { get; set; }

        [Display(Name = "MU_UnitType")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [ChoiceList(new object[] { "Pure", "Time", "Distance", "Count", "Mass", "Volume", "Money" },
            new string[] { "MU_Pure", "MU_Time", "MU_Distance", "MU_Count", "MU_Mass", "MU_Volume", "MU_Money" })]
        public string UnitType { get; set; }

        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [Display(Name = "MU_UnitAmount")]
        public double? UnitAmount { get; set; }

        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [Display(Name = "MU_BaseAmount")]
        public double? BaseAmount { get; set; }
    }

    public class MeasurementUnit : MeasurementUnitForSave
    {
        [AlwaysAccessible]
        [Display(Name = "IsActive")]
        public bool? IsActive { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")] 
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
