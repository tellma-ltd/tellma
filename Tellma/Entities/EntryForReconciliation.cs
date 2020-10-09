using System;
using System.ComponentModel.DataAnnotations;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "Entry", Plural = "Entries")]
    public class EntryForReconciliation : EntityWithKey<int>
    {
        [Display(Name = "Line_PostingDate")]
        public DateTime? PostingDate { get; set; }

        [Display(Name = "Entry_Direction")]
        [AlwaysAccessible]
        [ChoiceList(new object[] { (short)1, (short)-1 }, new string[] { "Entry_Direction_Debit", "Entry_Direction_Credit" })]
        public short? Direction { get; set; }

        [Display(Name = "Entry_MonetaryValue")]
        public decimal? MonetaryValue { get; set; }

        [Display(Name = "Entry_ExternalReference")]
        [StringLength(50)]
        public string ExternalReference { get; set; }
    }
}
