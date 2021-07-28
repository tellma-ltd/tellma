using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "Unit", GroupName = "Units")]
    public class UnitForSave : EntityWithKey<int>
    {
        [Display(Name = "Unit_UnitType")]
        [Required, ValidateRequired]
        [ChoiceList(new object[] { UnitTypes.Pure, UnitTypes.Time, UnitTypes.Distance, UnitTypes.Count, UnitTypes.Mass, UnitTypes.Volume },
            new string[] { UnitTypeNames.Pure, UnitTypeNames.Time, UnitTypeNames.Distance, UnitTypeNames.Count, UnitTypeNames.Mass, UnitTypeNames.Volume })]
        public string UnitType { get; set; }

        [Display(Name = "Name")]
        [Required, ValidateRequired]
        [StringLength(50)]
        public string Name { get; set; }

        [Display(Name = "Name")]
        [StringLength(50)]
        public string Name2 { get; set; }

        [Display(Name = "Name")]
        [StringLength(50)]
        public string Name3 { get; set; }

        [Display(Name = "Code")]
        [StringLength(50)]
        public string Code { get; set; }

        [Display(Name = "Description")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string Description { get; set; }

        [Display(Name = "Description")]
        [StringLength(255)]
        public string Description2 { get; set; }

        [Display(Name = "Description")]
        [StringLength(255)]
        public string Description3 { get; set; }

        [Display(Name = "Unit_UnitAmount")]
        [Required, ValidateRequired]
        public double? UnitAmount { get; set; }

        [Display(Name = "Unit_BaseAmount")]
        [Required, ValidateRequired]
        public double? BaseAmount { get; set; }
    }

    public class Unit : UnitForSave
    {
        [Display(Name = "IsActive")]
        [Required]
        public bool? IsActive { get; set; }

        [Display(Name = "CreatedAt")]
        [Required]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [Required]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [Required]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }

    public static class UnitTypes
    {
        public const string Pure = nameof(Pure);
        public const string Time = nameof(Time);
        public const string Distance = nameof(Distance);
        public const string Count = nameof(Count);
        public const string Mass = nameof(Mass);
        public const string Volume = nameof(Volume);
    }

    public static class UnitTypeNames
    {
        private const string _prefix = "Unit_";

        public const string Pure = _prefix + nameof(Pure);
        public const string Time = _prefix + nameof(Time);
        public const string Distance = _prefix + nameof(Distance);
        public const string Count = _prefix + nameof(Count);
        public const string Mass = _prefix + nameof(Mass);
        public const string Volume = _prefix + nameof(Volume);
    }
}
