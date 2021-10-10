using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "DocumentLineDefinitionEntry", GroupName = "DocumentLineDefinitionEntries")]
    public class DocumentLineDefinitionEntryForSave : EntityWithKey<int>
    {
        [Required, ValidateRequired]
        public int? LineDefinitionId { get; set; }

        [ValidateRequired]
        public int? EntryIndex { get; set; }

        [Display(Name = "Line_PostingDate")]
        public DateTime? PostingDate { get; set; }

        [IsCommonDisplay(Name = "Line_PostingDate")]
        [Required]
        public bool? PostingDateIsCommon { get; set; }

        [Display(Name = "Memo")]
        [StringLength(255)]
        public string Memo { get; set; }

        [IsCommonDisplay(Name = "Memo")]
        [DefaultValue(true)]
        [Required]
        public bool? MemoIsCommon { get; set; }

        [Display(Name = "Entry_Currency")]
        [StringLength(3)]
        public string CurrencyId { get; set; }

        [IsCommonDisplay(Name = "Entry_Currency")]
        [Required]
        public bool? CurrencyIsCommon { get; set; }

        [Display(Name = "Entry_Center")]
        public int? CenterId { get; set; }

        [IsCommonDisplay(Name = "Entry_Center")]
        [Required]
        public bool? CenterIsCommon { get; set; }

        [Display(Name = "Entry_Agent")]
        public int? AgentId { get; set; }

        [IsCommonDisplay(Name = "Entry_Agent")]
        [Required]
        public bool? AgentIsCommon { get; set; }

        [Display(Name = "Entry_NotedAgent")]
        public int? NotedAgentId { get; set; }

        [IsCommonDisplay(Name = "Entry_NotedAgent")]
        [Required]
        public bool? NotedAgentIsCommon { get; set; }

        [Display(Name = "Entry_Resource")]
        public int? ResourceId { get; set; }

        [IsCommonDisplay(Name = "Entry_Resource")]
        [Required]
        public bool? ResourceIsCommon { get; set; }

        [Display(Name = "Entry_NotedResource")]
        public int? NotedResourceId { get; set; }

        [IsCommonDisplay(Name = "Entry_NotedResource")]
        [Required]
        public bool? NotedResourceIsCommon { get; set; }

        [Display(Name = "Entry_Quantity")]
        public decimal? Quantity { get; set; }

        [IsCommonDisplay(Name = "Entry_Quantity")]
        [Required]
        public bool? QuantityIsCommon { get; set; }

        [Display(Name = "Entry_Unit")]
        public int? UnitId { get; set; }

        [IsCommonDisplay(Name = "Entry_Unit")]
        [Required]
        public bool? UnitIsCommon { get; set; }

        [Display(Name = "Entry_Time1")]
        [DataType(DataType.DateTime)]
        public DateTime? Time1 { get; set; }

        [IsCommonDisplay(Name = "Entry_Time1")]
        [Required]
        public bool? Time1IsCommon { get; set; }

        [Display(Name = "Entry_Duration")]
        public decimal? Duration { get; set; }

        [IsCommonDisplay(Name = "Entry_Duration")]
        [Required]
        public bool? DurationIsCommon { get; set; }

        [Display(Name = "Entry_DurationUnit")]
        public int? DurationUnitId { get; set; }

        [IsCommonDisplay(Name = "Entry_DurationUnit")]
        [Required]
        public bool? DurationUnitIsCommon { get; set; }

        [Display(Name = "Entry_Time2")]
        [DataType(DataType.DateTime)]
        public DateTime? Time2 { get; set; }

        [IsCommonDisplay(Name = "Entry_Time2")]
        [Required]
        public bool? Time2IsCommon { get; set; }

        [Display(Name = "Entry_ExternalReference")]
        [StringLength(50)]
        public string ExternalReference { get; set; }

        [IsCommonDisplay(Name = "Entry_ExternalReference")]
        [Required]
        public bool? ExternalReferenceIsCommon { get; set; }

        [Display(Name = "Entry_ReferenceSource")]
        public int? ReferenceSourceId { get; set; }

        [IsCommonDisplay(Name = "Entry_ReferenceSource")]
        [Required]
        public bool? ReferenceSourceIsCommon { get; set; }

        [Display(Name = "Entry_InternalReference")]
        [StringLength(50)]
        public string InternalReference { get; set; }

        [IsCommonDisplay(Name = "Entry_InternalReference")]
        [Required]
        public bool? InternalReferenceIsCommon { get; set; }
    }

    public class DocumentLineDefinitionEntry : DocumentLineDefinitionEntryForSave
    {
        [Required]
        public int? DocumentId { get; set; }

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

        // For Query

        [ForeignKey(nameof(LineDefinitionId))]
        public LineDefinition LineDefinition { get; set; }

        [Display(Name = "Entry_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "Entry_Center")]
        [ForeignKey(nameof(CenterId))]
        public Center Center { get; set; }

        [Display(Name = "Entry_Agent")]
        [ForeignKey(nameof(AgentId))]
        public Agent Agent { get; set; }

        [Display(Name = "Entry_NotedAgent")]
        [ForeignKey(nameof(NotedAgentId))]
        public Agent NotedAgent { get; set; }

        [Display(Name = "Entry_Resource")]
        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; }

        [Display(Name = "Entry_NotedResource")]
        [ForeignKey(nameof(NotedResourceId))]
        public Resource NotedResource { get; set; }

        [Display(Name = "Entry_Unit")]
        [ForeignKey(nameof(UnitId))]
        public Unit Unit { get; set; }

        [Display(Name = "Entry_DurationUnit")]
        [ForeignKey(nameof(DurationUnitId))]
        public Unit DurationUnit { get; set; }

        [Display(Name = "Entry_ReferenceSource")]
        [ForeignKey(nameof(ReferenceSourceId))]
        public Agent ReferenceSource { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
