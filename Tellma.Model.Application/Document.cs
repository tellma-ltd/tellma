using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "Document", GroupName = "Documents")]
    public class DocumentForSave<TDocumentLine, TLineDefinitionEntry, TAttachment> : EntityWithKey<int>
    {
        [Display(Name = "Document_SerialNumber")]
        [Required]
        [UserKey]
        public int? SerialNumber { get; set; }

        [Display(Name = "Document_Clearance")]
        [Required]
        [ChoiceList(new object[] { (byte)0, (byte)1, (byte)2 },
            new string[] { "Document_Clearance_0", "Document_Clearance_1", "Document_Clearance_2" })]
        public byte? Clearance { get; set; }

        [Display(Name = "Document_PostingDate")]
        public DateTime? PostingDate { get; set; }

        [IsCommonDisplay(Name = "Document_PostingDate")]
        [Required]
        public bool? PostingDateIsCommon { get; set; }

        [Display(Name = "Memo")]
        [StringLength(255)]
        public string Memo { get; set; }

        [IsCommonDisplay(Name = "Memo")]
        [Required]
        [DefaultValue(true)]
        public bool? MemoIsCommon { get; set; }

        [Display(Name = "Entry_Currency")]
        [StringLength(3)]
        public string CurrencyId { get; set; }

        [IsCommonDisplay(Name = "Entry_Currency")]
        [Required]
        public bool? CurrencyIsCommon { get; set; }

        [Display(Name = "Document_Center")]
        public int? CenterId { get; set; }

        [IsCommonDisplay(Name = "Document_Center")]
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

        [ForeignKey(nameof(Line.DocumentId))]
        public List<TDocumentLine> Lines { get; set; }

        [ForeignKey(nameof(DocumentLineDefinitionEntry.DocumentId))]
        public List<TLineDefinitionEntry> LineDefinitionEntries { get; set; }

        [Display(Name = "Document_Attachments")]
        [ForeignKey(nameof(Attachment.DocumentId))]
        public List<TAttachment> Attachments { get; set; }
    }

    public class DocumentForSave : DocumentForSave<LineForSave, DocumentLineDefinitionEntryForSave, AttachmentForSave>
    {

    }

    public class Document : DocumentForSave<Line, DocumentLineDefinitionEntry, Attachment>
    {
        [Display(Name = "Definition")]
        [Required]
        public int? DefinitionId { get; set; }

        [Display(Name = "Code")]
        [Required]
        public string Code { get; set; }

        [Display(Name = "Document_State")]
        [Required]
        [ChoiceList(new object[] {
            0,
            1,
            -1,
        },
            new string[] {
            "Document_State_0",
            "Document_State_1",
            "Document_State_minus_1",
        })]
        public short? State { get; set; }

        [Display(Name = "Document_StateAt")]
        [Required]
        public DateTimeOffset? StateAt { get; set; }

        [Display(Name = "Document_Comment")]
        public string Comment { get; set; }

        [Display(Name = "Document_Assignee")]
        public int? AssigneeId { get; set; }

        [Display(Name = "Document_AssignedAt")]
        public DateTimeOffset? AssignedAt { get; set; }

        [Display(Name = "Document_AssignedBy")]
        public int? AssignedById { get; set; }

        [Display(Name = "Document_OpenedAt")]
        public DateTimeOffset? OpenedAt { get; set; }

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

        [Display(Name = "Entry_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "Document_Center")]
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

        [Display(Name = "Entry_Unit")]
        [ForeignKey(nameof(UnitId))]
        public Unit Unit { get; set; }

        [Display(Name = "Entry_DurationUnit")]
        [ForeignKey(nameof(DurationUnitId))]
        public Unit DurationUnit { get; set; }

        [Display(Name = "Entry_ReferenceSource")]
        [ForeignKey(nameof(ReferenceSourceId))]
        public Agent ReferenceSource { get; set; }

        [Display(Name = "Definition")]
        [ForeignKey(nameof(DefinitionId))]
        public DocumentDefinition Definition { get; set; }

        [Display(Name = "Document_Assignee")]
        [ForeignKey(nameof(AssigneeId))]
        public User Assignee { get; set; }

        [Display(Name = "Document_AssignedBy")]
        [ForeignKey(nameof(AssignedById))]
        public User AssignedBy { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }

        [Display(Name = "Document_AssignmentsHistory")]
        [ForeignKey(nameof(DocumentAssignment.DocumentId))]
        public List<DocumentAssignment> AssignmentsHistory { get; set; }

        [Display(Name = "Document_StatesHistory")]
        [ForeignKey(nameof(DocumentStateChange.DocumentId))]
        public List<DocumentStateChange> StatesHistory { get; set; }
    }
}
