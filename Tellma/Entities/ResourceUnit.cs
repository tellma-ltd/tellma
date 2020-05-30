using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "ResourceUnit", Plural = "ResourceUnits")]
    public class ResourceUnitForSave : EntityWithKey<int>
    {
        [Display(Name = "ResourceUnit_Unit")]
        public int? UnitId { get; set; }

        [Display(Name = "ResourceUnit_Multiplier")]
        public double? Multiplier { get; set; }
    }

    public class ResourceUnit : ResourceUnitForSave
    {
        public int? ResourceId { get; set; }

        // For Query

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "ResourceUnit_Unit")]
        [ForeignKey(nameof(UnitId))]
        public Unit Unit { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }

    }
}
