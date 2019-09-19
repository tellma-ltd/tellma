using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
{
    [StrongEntity]
    public class ResourceForSave : EntityWithKey<int>
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
        public string Code { get; set; } // IsVisible (Yes), Label (No), IsRequired (Yes), DefaultValue (No) (make sure 

        [Display(Name = "Resource_Classification")]
        public int? ResourceClassificationId { get; set; } // IsVisible (Yes), Label (Yes), IsRequired (Yes), DefaultValue (No)

        [Display(Name = "Resource_Currency")]
        public string CurrencyId { get; set; } 

        [Display(Name = "Resource_MassUnit")]
        public int? MassUnitId { get; set; }

        [Display(Name = "Resource_VolumeUnit")]
        public int? VolumeUnitId { get; set; }

        [Display(Name = "Resource_AreaUnit")]
        public int? AreaUnitId { get; set; }

        [Display(Name = "Resource_LengthUnit")]
        public int? LengthUnitId { get; set; }

        [Display(Name = "Resource_TimeUnit")]
        public int? TimeUnitId { get; set; }

        [Display(Name = "Resource_CountUnit")]
        public int? CountUnitId { get; set; }

        [Display(Name = "Memo")]
        [StringLength(2048, ErrorMessage = nameof(StringLengthAttribute))]
        public string Memo { get; set; } // 3 values

        [Display(Name = "Resource_CustomsReference")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string CustomsReference { get; set; } // 3 values

        [Display(Name = "Resource_ResourceLookup1")]
        public int? ResourceLookup1Id { get; set; }

        [Display(Name = "Resource_ResourceLookup2")]
        public int? ResourceLookup2Id { get; set; }

        [Display(Name = "Resource_ResourceLookup3")]
        public int? ResourceLookup3Id { get; set; }

        [Display(Name = "Resource_ResourceLookup4")]
        public int? ResourceLookup4Id { get; set; }
    }

    public class Resource : ResourceForSave
    {
        public string ResourceDefinitionId { get; set; }

        [Display(Name = "IsActive")]
        [AlwaysAccessible]
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

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }

        // TODO Classification

        [Display(Name = "Resource_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "Resource_MassUnit")]
        [ForeignKey(nameof(MassUnitId))]
        public MeasurementUnit MassUnit { get; set; }

        [Display(Name = "Resource_VolumeUnit")]
        [ForeignKey(nameof(VolumeUnitId))]
        public MeasurementUnit VolumeUnit { get; set; }

        [Display(Name = "Resource_AreaUnit")]
        [ForeignKey(nameof(AreaUnitId))]
        public MeasurementUnit AreaUnit { get; set; }

        [Display(Name = "Resource_LengthUnit")]
        [ForeignKey(nameof(LengthUnitId))]
        public MeasurementUnit LengthUnit { get; set; }

        [Display(Name = "Resource_TimeUnit")]
        [ForeignKey(nameof(TimeUnitId))]
        public MeasurementUnit TimeUnit { get; set; }

        [Display(Name = "Resource_CountUnit")]
        [ForeignKey(nameof(CountUnitId))]
        public MeasurementUnit CountUnit { get; set; }

        [Display(Name = "Resource_ResourceLookup1")]
        [ForeignKey(nameof(ResourceLookup1Id))]
        public ResourceLookup ResourceLookup1 { get; set; }

        [Display(Name = "Resource_ResourceLookup2")]
        [ForeignKey(nameof(ResourceLookup2Id))]
        public ResourceLookup ResourceLookup2 { get; set; }

        [Display(Name = "Resource_ResourceLookup3")]
        [ForeignKey(nameof(ResourceLookup3Id))]
        public ResourceLookup ResourceLookup3 { get; set; }

        [Display(Name = "Resource_ResourceLookup4")]
        [ForeignKey(nameof(ResourceLookup4Id))]
        public ResourceLookup ResourceLookup4 { get; set; }
    }
}
