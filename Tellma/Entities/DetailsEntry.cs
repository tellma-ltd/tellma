using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "DetailsEntry", Plural = "DetailsEntries")]
    public class DetailsEntry : EntityWithKey<int>
    {
        [Display(Name = "Entry_Line")]
        public int? LineId { get; set; }

        [Display(Name = "Entry_Center")]
        public int? CenterId { get; set; }

        [Display(Name = "Entry_Direction")]
        [ChoiceList(new object[] { (short)-1, (short)1 })]
        public short? Direction { get; set; }

        [Display(Name = "Entry_Account")]
        public int? AccountId { get; set; }

        [Display(Name = "Entry_Custodian")]
        public int? CustodianId { get; set; }

        [Display(Name = "Entry_Custody")]
        public int? CustodyId { get; set; }

        [Display(Name = "Entry_EntryType")]
        public int? EntryTypeId { get; set; }

        [Display(Name = "Entry_Participant")]
        public int? ParticipantId { get; set; }

        [Display(Name = "Entry_Resource")]
        public int? ResourceId { get; set; }

        [Display(Name = "Entry_Quantity")]
        public decimal? Quantity { get; set; }

        [Display(Name = "DetailsEntry_AlgebraicQuantity")]
        public decimal? AlgebraicQuantity { get; set; }

        [Display(Name = "DetailsEntry_NegativeAlgebraicQuantity")]
        public decimal? NegativeAlgebraicQuantity { get; set; }

        [Display(Name = "Entry_Unit")]
        public int? UnitId { get; set; }

        [Display(Name = "Entry_MonetaryValue")]
        public decimal? MonetaryValue { get; set; }

        [Display(Name = "DetailsEntry_AlgebraicMonetaryValue")]
        public decimal? AlgebraicMonetaryValue { get; set; }

        [Display(Name = "DetailsEntry_NegativeAlgebraicMonetaryValue")]
        public decimal? NegativeAlgebraicMonetaryValue { get; set; }

        [Display(Name = "Entry_Currency")]
        public string CurrencyId { get; set; }

        [Display(Name = "DetailsEntry_Count")]
        public decimal? Count { get; set; }

        [Display(Name = "DetailsEntry_AlgebraicCount")]
        public decimal? AlgebraicCount { get; set; }

        [Display(Name = "DetailsEntry_Mass")]
        public decimal? Mass { get; set; }

        [Display(Name = "DetailsEntry_AlgebraicMass")]
        public decimal? AlgebraicMass { get; set; }

        [Display(Name = "DetailsEntry_Volume")]
        public decimal? Volume { get; set; }

        [Display(Name = "DetailsEntry_AlgebraicVolume")]
        public decimal? AlgebraicVolume { get; set; }

        [Display(Name = "DetailsEntry_Time")]
        public decimal? Time { get; set; }

        [Display(Name = "DetailsEntry_AlgebraicTime")]
        public decimal? AlgebraicTime { get; set; }

        [Display(Name = "Entry_Value")]
        public decimal? Value { get; set; }

        [Display(Name = "DetailsEntry_Actual")]
        public decimal? Actual { get; set; }

        [Display(Name = "DetailsEntry_Planned")]
        public decimal? Planned { get; set; }

        [Display(Name = "DetailsEntry_Variance")]
        public decimal? Variance { get; set; }

        [Display(Name = "Entry_AlgebraicValue")]
        public decimal? AlgebraicValue { get; set; }

        [Display(Name = "Entry_NegativeAlgebraicValue")]
        public decimal? NegativeAlgebraicValue { get; set; }

        [Display(Name = "DetailsEntry_MonetaryValuePerUnit")]
        public decimal? MonetaryValuePerUnit { get; set; }

        [Display(Name = "DetailsEntry_ValuePerUnit")]
        public decimal? ValuePerUnit { get; set; }

        [Display(Name = "Entry_Time1")]
        [IncludesTime]
        public DateTime? Time1 { get; set; }

        [Display(Name = "Entry_Time2")]
        [IncludesTime]
        public DateTime? Time2 { get; set; }

        [Display(Name = "Entry_ExternalReference")]
        public string ExternalReference { get; set; }

        [Display(Name = "Entry_InternalReference")]
        public string InternalReference { get; set; }

        [Display(Name = "Entry_NotedAgentName")]
        public string NotedAgentName { get; set; }

        [Display(Name = "Entry_NotedAmount")]
        public decimal? NotedAmount { get; set; }

        [Display(Name = "Entry_NotedDate")]
        public DateTime? NotedDate { get; set; }

        [NotMapped]
        [Display(Name = "DetailsEntry_Accumulation")]
        public decimal? Accumulation { get; set; }

        [NotMapped]
        [Display(Name = "DetailsEntry_QuantityAccumulation")]
        public decimal? QuantityAccumulation { get; set; }

        [NotMapped]
        [Display(Name = "DetailsEntry_MonetaryValueAccumulation")]
        public decimal? MonetaryValueAccumulation { get; set; }

        // For Query

        [Display(Name = "Entry_Line")]
        [ForeignKey(nameof(LineId))]
        public LineForQuery Line { get; set; }

        [Display(Name = "Entry_Account")]
        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; }

        [Display(Name = "Entry_EntryType")]
        [ForeignKey(nameof(EntryTypeId))]
        public EntryType EntryType { get; set; }

        [Display(Name = "Entry_Custodian")]
        [ForeignKey(nameof(CustodianId))]
        public Relation Custodian { get; set; }

        [Display(Name = "Entry_Custody")]
        [ForeignKey(nameof(CustodyId))]
        public Custody Custody { get; set; }

        [Display(Name = "Entry_Center")]
        [ForeignKey(nameof(CenterId))]
        public Center Center { get; set; }

        [Display(Name = "Entry_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "Entry_Participant")]
        [ForeignKey(nameof(ParticipantId))]
        public Relation Participant { get; set; }

        [Display(Name = "Entry_Resource")]
        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; }

        [Display(Name = "Entry_Unit")]
        [ForeignKey(nameof(UnitId))]
        public Unit Unit { get; set; }
    }
}
