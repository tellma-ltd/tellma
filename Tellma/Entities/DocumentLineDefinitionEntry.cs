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
        public int? LineDefinitionId { get; set; }

        [Required]
        public int? EntryIndex { get; set; }

        [Display(Name = "Line_PostingDate")]
        public DateTime? PostingDate { get; set; }

        [IsCommonDisplay(Name = "Line_PostingDate")]
        public bool? PostingDateIsCommon { get; set; }

        [Display(Name = "Memo")]
        [StringLength(255)]
        public string Memo { get; set; }

        [IsCommonDisplay(Name = "Memo")]
        [DefaultValue(true)]
        public bool? MemoIsCommon { get; set; }

        [Display(Name = "Entry_Currency")]
        [StringLength(3)]
        public string CurrencyId { get; set; }

        [IsCommonDisplay(Name = "Entry_Currency")]
        public bool? CurrencyIsCommon { get; set; }

        [Display(Name = "Entry_Center")]
        public int? CenterId { get; set; }

        [IsCommonDisplay(Name = "Entry_Center")]
        public bool? CenterIsCommon { get; set; }

        [Display(Name = "Entry_Custodian")]
        public int? CustodianId { get; set; }

        [IsCommonDisplay(Name = "Entry_Custodian")]
        public bool? CustodianIsCommon { get; set; }

        [Display(Name = "Entry_Custody")]
        public int? CustodyId { get; set; }

        [IsCommonDisplay(Name = "Entry_Custody")]
        public bool? CustodyIsCommon { get; set; }

        [Display(Name = "Entry_Participant")]
        public int? ParticipantId { get; set; }

        [IsCommonDisplay(Name = "Entry_Participant")]
        public bool? ParticipantIsCommon { get; set; }

        [Display(Name = "Entry_Resource")]
        public int? ResourceId { get; set; }

        [IsCommonDisplay(Name = "Entry_Resource")]
        public bool? ResourceIsCommon { get; set; }

        [Display(Name = "Entry_Quantity")]
        public decimal? Quantity { get; set; }

        [IsCommonDisplay(Name = "Entry_Quantity")]
        public bool? QuantityIsCommon { get; set; }

        [Display(Name = "Entry_Unit")]
        public int? UnitId { get; set; }

        [IsCommonDisplay(Name = "Entry_Unit")]
        public bool? UnitIsCommon { get; set; }

        [Display(Name = "Entry_Time1")]
        public DateTime? Time1 { get; set; }

        [IsCommonDisplay(Name = "Entry_Time1")]
        public bool? Time1IsCommon { get; set; }

        [Display(Name = "Entry_Time2")]
        public DateTime? Time2 { get; set; }

        [IsCommonDisplay(Name = "Entry_Time2")]
        public bool? Time2IsCommon { get; set; }

        [Display(Name = "Entry_ExternalReference")]
        [StringLength(50)]
        public string ExternalReference { get; set; }

        [IsCommonDisplay(Name = "Entry_ExternalReference")]
        public bool? ExternalReferenceIsCommon { get; set; }

        [Display(Name = "Entry_AdditionalReference")]
        [StringLength(50)]
        public string AdditionalReference { get; set; }

        [IsCommonDisplay(Name = "Entry_AdditionalReference")]
        public bool? AdditionalReferenceIsCommon { get; set; }
    }

    public class DocumentLineDefinitionEntry : DocumentLineDefinitionEntryForSave
    {
        public int? DocumentId { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For Query

        [ForeignKey(nameof(LineDefinitionId))]
        public LineDefinition Definition { get; set; }

        [Display(Name = "Entry_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "Entry_Custodian")]
        [ForeignKey(nameof(CustodianId))]
        public Relation Custodian { get; set; }

        [Display(Name = "Entry_Custody")]
        [ForeignKey(nameof(CustodyId))]
        public Custody Custody { get; set; }

        [Display(Name = "Entry_Participant")]
        [ForeignKey(nameof(ParticipantId))]
        public Relation Participant { get; set; }

        [Display(Name = "Entry_Resource")]
        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; }

        [Display(Name = "Entry_Unit")]
        [ForeignKey(nameof(UnitId))]
        public Unit Unit { get; set; }

        [Display(Name = "Entry_Center")]
        [ForeignKey(nameof(CenterId))]
        public Center Center { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
