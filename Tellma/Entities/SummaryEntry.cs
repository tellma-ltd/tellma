using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "SummaryEntry", Plural = "SummaryEntries")]
    public class SummaryEntry : Entity
    {
        [Display(Name = "Entry_Account")]
        public int? AccountId { get; set; }

        [Display(Name = "Entry_Account")]
        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; }

        [Display(Name = "Entry_Center")]
        public int? CenterId { get; set; }

        [Display(Name = "Entry_Center")]
        [ForeignKey(nameof(CenterId))]
        public Center Center { get; set; }

        [Display(Name = "Entry_Currency")]
        public string CurrencyId { get; set; }

        [Display(Name = "Entry_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "Entry_Participant")]
        public int? ParticipantId { get; set; }

        [Display(Name = "Entry_Participant")]
        [ForeignKey(nameof(ParticipantId))]
        public Relation Participant { get; set; }

        [Display(Name = "Entry_Resource")]
        public int? ResourceId { get; set; }

        [Display(Name = "Entry_Resource")]
        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; }

        [Display(Name = "Entry_Unit")]
        public int? UnitId { get; set; }

        [Display(Name = "Entry_Unit")]
        [ForeignKey(nameof(UnitId))]
        public Unit Unit { get; set; }

        [Display(Name = "Entry_Custodian")]
        public int? CustodianId { get; set; }

        [Display(Name = "Entry_Custodian")]
        [ForeignKey(nameof(CustodianId))]
        public Relation Custodian { get; set; }

        [Display(Name = "Entry_Custody")]
        public int? CustodyId { get; set; }

        [Display(Name = "Entry_Custody")]
        [ForeignKey(nameof(CustodyId))]
        public Custody Custody { get; set; }

        [Display(Name = "Entry_EntryType")]
        public int? EntryTypeId { get; set; }

        [Display(Name = "Entry_EntryType")]
        [ForeignKey(nameof(EntryTypeId))]
        public EntryType EntryType { get; set; }

        // Quantity

        [Display(Name = "SummaryEntry_OpeningQuantity")]
        public decimal? OpeningQuantity { get; set; }

        [Display(Name = "SummaryEntry_QuantityIn")]
        public decimal? QuantityIn { get; set; }

        [Display(Name = "SummaryEntry_QuantityOut")]
        public decimal? QuantityOut { get; set; }

        [Display(Name = "SummaryEntry_ClosingQuantity")]
        public decimal? ClosingQuantity { get; set; }

        // Mass

        [Display(Name = "SummaryEntry_OpeningMass")]
        public decimal? OpeningMass { get; set; }

        [Display(Name = "SummaryEntry_MassIn")]
        public decimal? MassIn { get; set; }

        [Display(Name = "SummaryEntry_MassOut")]
        public decimal? MassOut { get; set; }

        [Display(Name = "SummaryEntry_ClosingMass")]
        public decimal? ClosingMass { get; set; }

        // Monetary Value

        [Display(Name = "SummaryEntry_OpeningMonetaryValue")]
        public decimal? OpeningMonetaryValue { get; set; }

        [Display(Name = "SummaryEntry_MonetaryValueIn")]
        public decimal? MonetaryValueIn { get; set; }

        [Display(Name = "SummaryEntry_MonetaryValueOut")]
        public decimal? MonetaryValueOut { get; set; }

        [Display(Name = "SummaryEntry_ClosingMonetaryValue")]
        public decimal? ClosingMonetaryValue { get; set; }

        // Value

        [Display(Name = "SummaryEntry_Opening")]
        public decimal? Opening { get; set; }

        [Display(Name = "SummaryEntry_Debit")]
        public decimal? Debit { get; set; }

        [Display(Name = "SummaryEntry_Credit")]
        public decimal? Credit { get; set; }

        [Display(Name = "SummaryEntry_Closing")]
        public decimal? Closing { get; set; }
    }
}
