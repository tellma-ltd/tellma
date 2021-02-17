using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// I will put here the readonly entities added to build the JV

namespace Tellma.Entities
{
    [StrongEntity]
    public class VoucherBookletForSave : EntityWithKey<int>
    {
        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [Required]
        [NotNull]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name2 { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name3 { get; set; }

        // Temp

        public int VoucherTypeId { get; set; }
        
        [NotNull]
        public string StringPrefix { get; set; }
        public int? NumericLength { get; set; }
        public int? RangeStarts { get; set; }
        public int? RangeEnds { get; set; }
    }

    public class VoucherBooklet : VoucherBookletForSave
    {
        [Display(Name = "IsActive")]
        [AlwaysAccessible]
        public bool? IsActive { get; set; }

        //[Display(Name = "CreatedAt")]
        //[NotNull]
        //public DateTimeOffset? CreatedAt { get; set; }

        //[Display(Name = "CreatedBy")]
        //[NotNull]
        //public int? CreatedById { get; set; }

        //[Display(Name = "ModifiedAt")]
        //[NotNull]
        //public DateTimeOffset? ModifiedAt { get; set; }

        //[Display(Name = "ModifiedBy")]
        //[NotNull]
        //public int? ModifiedById { get; set; }

        //[Display(Name = "CreatedBy")]
        //[ForeignKey(nameof(CreatedById))]
        //public User CreatedBy { get; set; }

        //[Display(Name = "CreatedBy")]
        //[ForeignKey(nameof(ModifiedById))]
        //public User ModifiedBy { get; set; }


        // Temp

        //[ForeignKey(nameof(VoucherTypeId))]
        //public VoucherType VoucherType { get; set; }
    }
}
