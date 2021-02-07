using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "Currency", Plural = "Currencies")]
    public class CurrencyForSave : EntityWithKey<string>
    {
        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [Required]
        [NotNull]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Name { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Name2 { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Name3 { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Primary)]
        [Required]
        [NotNull]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Description { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Secondary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Description2 { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Ternary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Description3 { get; set; }

        [Display(Name = "Currency_NumericCode")]
        [NotNull]
        public short? NumericCode { get; set; }

        [Display(Name = "Currency_DecimalPlaces")]
        [Required]
        [NotNull]
        [ChoiceList(new object[] { (short)0, (short)2, (short)3 })]
        public short? E { get; set; }
    }

    public class Currency : CurrencyForSave
    {
        [AlwaysAccessible]
        [NotNull]
        [Display(Name = "IsActive")]
        public bool? IsActive { get; set; }

        [Display(Name = "CreatedAt")]
        [NotNull]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [NotNull]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [NotNull]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [NotNull]
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
