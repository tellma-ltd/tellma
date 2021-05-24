using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "Currency", GroupName = "Currencies")]
    public class CurrencyForSave : EntityWithKey<string>
    {
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

        [Display(Name = "Currency_NumericCode")]
        [Required]
        public short? NumericCode { get; set; }

        [Display(Name = "Currency_DecimalPlaces")]
        [Required]
        [ChoiceList(new object[] { (short)0, (short)2, (short)3 })]
        public short? E { get; set; }
    }

    public class Currency : CurrencyForSave
    {

        [Required]
        [Display(Name = "IsActive")]
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

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
