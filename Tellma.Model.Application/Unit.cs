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
        [Required]
        [ChoiceList(new object[] { "Pure", "Time", "Distance", "Count", "Mass", "Volume" },
            new string[] { "Unit_Pure", "Unit_Time", "Unit_Distance", "Unit_Count", "Unit_Mass", "Unit_Volume" })]
        public string UnitType { get; set; }

        [Display(Name = "Name")]
        [Required]
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
        [Required]
        [StringLength(255)]
        public string Description { get; set; }

        [Display(Name = "Description")]
        [StringLength(255)]
        public string Description2 { get; set; }

        [Display(Name = "Description")]
        [StringLength(255)]
        public string Description3 { get; set; }

        [Display(Name = "Unit_UnitAmount")]
        [Required]
        public double? UnitAmount { get; set; }

        [Display(Name = "Unit_BaseAmount")]
        [Required]
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
}
