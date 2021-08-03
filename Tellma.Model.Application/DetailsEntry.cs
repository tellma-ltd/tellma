using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "DetailsEntry", GroupName = "DetailsEntries")]
    public class DetailsEntry : EntityWithKey<int>
    {
        [Display(Name = "Entry_Line")]
        [Required]
        public int? LineId { get; set; }

        [Display(Name = "Entry_Direction")]
        [ChoiceList(new object[] { (short)1, (short)-1 }, new string[] { "Entry_Direction_Debit", "Entry_Direction_Credit" })]
        [Required]
        public short? Direction { get; set; }

        [Display(Name = "Entry_Account")]
        public int? AccountId { get; set; }

        [Display(Name = "Entry_Currency")]
        [Required]
        public string CurrencyId { get; set; }

        [Display(Name = "Entry_Relation")]
        public int? RelationId { get; set; }

        [Display(Name = "Entry_Custodian")]
        public int? CustodianId { get; set; }

        [Display(Name = "Entry_NotedRelation")]
        public int? NotedRelationId { get; set; }

        [Display(Name = "Entry_Resource")]
        public int? ResourceId { get; set; }

        [Display(Name = "Entry_Center")]
        [Required]
        public int? CenterId { get; set; }

        [Display(Name = "Entry_EntryType")]
        public int? EntryTypeId { get; set; }

        [Display(Name = "Entry_MonetaryValue")]
        public decimal? MonetaryValue { get; set; }

        [Display(Name = "Entry_Quantity")]
        public decimal? Quantity { get; set; }

        [Display(Name = "Entry_Unit")]
        public int? UnitId { get; set; }

        [Display(Name = "Entry_Value")]
        public decimal? Value { get; set; }

        [Display(Name = "Entry_RValue")]
        public decimal? RValue { get; set; }

        [Display(Name = "Entry_PValue")]
        public decimal? PValue { get; set; }

        [Display(Name = "Entry_Time1")]
        [DataType(DataType.DateTime)]
        public DateTime? Time1 { get; set; }

        [Display(Name = "Entry_Duration")]
        public decimal? Duration { get; set; }

        [Display(Name = "Entry_DurationUnit")]
        public int? DurationUnitId { get; set; }

        [Display(Name = "Entry_Time2")]
        [DataType(DataType.DateTime)]
        public DateTime? Time2 { get; set; }

        [Display(Name = "Entry_ExternalReference")]
        public string ExternalReference { get; set; }

        [Display(Name = "Entry_ReferenceSource")]
        public int? ReferenceSourceId { get; set; }

        [Display(Name = "Entry_InternalReference")]
        public string InternalReference { get; set; }

        [Display(Name = "Entry_NotedAgentName")]
        public string NotedAgentName { get; set; }

        [Display(Name = "Entry_NotedAmount")]
        public decimal? NotedAmount { get; set; }

        [Display(Name = "Entry_NotedDate")]
        public DateTime? NotedDate { get; set; }

        [Display(Name = "DetailsEntry_BaseQuantity")]
        public decimal? BaseQuantity { get; set; }

        [Display(Name = "DetailsEntry_BaseUnit")]
        public int? BaseUnitId { get; set; }

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

        [Display(Name = "Entry_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "Entry_Relation")]
        [ForeignKey(nameof(RelationId))]
        public Relation Relation { get; set; }

        [Display(Name = "Entry_Custodian")]
        [ForeignKey(nameof(CustodianId))]
        public Relation Custodian { get; set; }

        [Display(Name = "Entry_NotedRelation")]
        [ForeignKey(nameof(NotedRelationId))]
        public Relation NotedRelation { get; set; }

        [Display(Name = "Entry_Resource")]
        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; }

        [Display(Name = "Entry_Center")]
        [ForeignKey(nameof(CenterId))]
        public Center Center { get; set; }

        [Display(Name = "Entry_EntryType")]
        [ForeignKey(nameof(EntryTypeId))]
        public EntryType EntryType { get; set; }

        [Display(Name = "Entry_Unit")]
        [ForeignKey(nameof(UnitId))]
        public Unit Unit { get; set; }

        [Display(Name = "Entry_DurationUnit")]
        [ForeignKey(nameof(DurationUnitId))]
        public Unit DurationUnit { get; set; }

        [Display(Name = "DetailsEntry_BaseUnit")]
        [ForeignKey(nameof(CurrencyId))]
        public Unit BaseUnit { get; set; }

        [Display(Name = "Entry_ReferenceSource")]
        [ForeignKey(nameof(ReferenceSourceId))]
        public Relation ReferenceSource { get; set; }
    }
}
