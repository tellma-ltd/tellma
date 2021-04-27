using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "DocumentLineDefinitionEntry", Plural = "DocumentLineDefinitionEntries")]
    public class DocumentLineDefinitionEntryForSave : EntityWithKey<int>
    {
        [Required]
        [NotNull]
        public int? LineDefinitionId { get; set; }

        [Required]
        public int? EntryIndex { get; set; }

        [Display(Name = "Line_PostingDate")]
        public DateTime? PostingDate { get; set; }

        [IsCommonDisplay(Name = "Line_PostingDate")]
        [NotNull]
        public bool? PostingDateIsCommon { get; set; }

        [Display(Name = "Memo")]
        [StringLength(255)]
        public string Memo { get; set; }

        [IsCommonDisplay(Name = "Memo")]
        [DefaultValue(true)]
        [NotNull]
        public bool? MemoIsCommon { get; set; }

        [Display(Name = "Entry_Currency")]
        [StringLength(3)]
        public string CurrencyId { get; set; }

        [IsCommonDisplay(Name = "Entry_Currency")]
        [NotNull]
        public bool? CurrencyIsCommon { get; set; }

        [Display(Name = "Entry_Center")]
        public int? CenterId { get; set; }

        [IsCommonDisplay(Name = "Entry_Center")]
        [NotNull]
        public bool? CenterIsCommon { get; set; }

        [Display(Name = "Entry_Relation")]
        public int? RelationId { get; set; }

        [IsCommonDisplay(Name = "Entry_Relation")]
        [NotNull]
        public bool? RelationIsCommon { get; set; }

        [Display(Name = "Entry_Custodian")]
        public int? CustodianId { get; set; }

        [IsCommonDisplay(Name = "Entry_Custodian")]
        [NotNull]
        public bool? CustodianIsCommon { get; set; }

        [Display(Name = "Entry_NotedRelation")]
        public int? NotedRelationId { get; set; }

        [IsCommonDisplay(Name = "Entry_NotedRelation")]
        [NotNull]
        public bool? NotedRelationIsCommon { get; set; }

        [Display(Name = "Entry_Resource")]
        public int? ResourceId { get; set; }

        [IsCommonDisplay(Name = "Entry_Resource")]
        [NotNull]
        public bool? ResourceIsCommon { get; set; }

        [Display(Name = "Entry_Quantity")]
        public decimal? Quantity { get; set; }

        [IsCommonDisplay(Name = "Entry_Quantity")]
        [NotNull]
        public bool? QuantityIsCommon { get; set; }

        [Display(Name = "Entry_Unit")]
        public int? UnitId { get; set; }

        [IsCommonDisplay(Name = "Entry_Unit")]
        [NotNull]
        public bool? UnitIsCommon { get; set; }

        [Display(Name = "Entry_Time1")]
        [IncludesTime]
        public DateTime? Time1 { get; set; }

        [IsCommonDisplay(Name = "Entry_Time1")]
        [NotNull]
        public bool? Time1IsCommon { get; set; }

        [Display(Name = "Entry_Duration")]
        public decimal? Duration { get; set; }

        [IsCommonDisplay(Name = "Entry_Duration")]
        [NotNull]
        public bool? DurationIsCommon { get; set; }

        [Display(Name = "Entry_DurationUnit")]
        public int? DurationUnitId { get; set; }

        [IsCommonDisplay(Name = "Entry_DurationUnit")]
        [NotNull]
        public bool? DurationUnitIsCommon { get; set; }

        [Display(Name = "Entry_Time2")]
        [IncludesTime]
        public DateTime? Time2 { get; set; }

        [IsCommonDisplay(Name = "Entry_Time2")]
        [NotNull]
        public bool? Time2IsCommon { get; set; }

        [Display(Name = "Entry_ExternalReference")]
        [StringLength(50)]
        public string ExternalReference { get; set; }

        [IsCommonDisplay(Name = "Entry_ExternalReference")]
        [NotNull]
        public bool? ExternalReferenceIsCommon { get; set; }

        [Display(Name = "Entry_ReferenceSource")]
        public int? ReferenceSourceId { get; set; }

        [IsCommonDisplay(Name = "Entry_ReferenceSource")]
        [NotNull]
        public bool? ReferenceSourceIsCommon { get; set; }

        [Display(Name = "Entry_InternalReference")]
        [StringLength(50)]
        public string InternalReference { get; set; }

        [IsCommonDisplay(Name = "Entry_InternalReference")]
        [NotNull]
        public bool? InternalReferenceIsCommon { get; set; }
    }

    public class DocumentLineDefinitionEntry : DocumentLineDefinitionEntryForSave
    {
        [NotNull]
        public int? DocumentId { get; set; }

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

        [ForeignKey(nameof(LineDefinitionId))]
        public LineDefinition LineDefinition { get; set; }

        [Display(Name = "Entry_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "Entry_Center")]
        [ForeignKey(nameof(CenterId))]
        public Center Center { get; set; }

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

        [Display(Name = "Entry_Unit")]
        [ForeignKey(nameof(UnitId))]
        public Unit Unit { get; set; }

        [Display(Name = "Entry_DurationUnit")]
        [ForeignKey(nameof(DurationUnitId))]
        public Unit DurationUnit { get; set; }

        [Display(Name = "Entry_ReferenceSource")]
        [ForeignKey(nameof(ReferenceSourceId))]
        public Relation ReferenceSource { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
