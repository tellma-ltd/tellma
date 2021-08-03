using System;
using System.ComponentModel.DataAnnotations;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "Entry", GroupName = "Entries")]
    public class EntryForReconciliation : EntityWithKey<int>
    {
        [Display(Name = "Line_PostingDate")]
        public DateTime? PostingDate { get; set; }

        [Display(Name = "Entry_Direction")]
        [ChoiceList(new object[] { (short)1, (short)-1 }, new string[] { "Entry_Direction_Debit", "Entry_Direction_Credit" })]
        public short? Direction { get; set; }

        [Display(Name = "Entry_MonetaryValue")]
        public decimal? MonetaryValue { get; set; }

        [Display(Name = "Entry_ExternalReference")]
        [StringLength(50)]
        public string ExternalReference { get; set; }

        public int? DocumentId { get; set; }

        public int? DocumentDefinitionId { get; set; }

        public int? DocumentSerialNumber { get; set; }

        public bool? IsReconciledLater { get; set; }
    }
}
