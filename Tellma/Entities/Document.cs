using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    public class DocumentForSave<TDocumentLine, TAttachment> : EntityWithKey<int>
    {
        [Display(Name = "Document_DocumentDate")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        public DateTime? DocumentDate { get; set; }

        [Display(Name = "Document_Clearance")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [ChoiceList(new object[] { (byte)0, (byte)1, (byte)2 },
            new string[] { "Document_Clearance_0", "Document_Clearance_1", "Document_Clearance_2" })]
        public byte? Clearance { get; set; }

        // HIDDEN

        [Display(Name = "Document_VoucherBooklet")]
        public int? VoucherBookletId { get; set; } // HIDDEN

        [Display(Name = "Document_VoucherNumericReference")]
        public int? VoucherNumericReference { get; set; }

        [Display(Name = "Document_DocumentLookup1")]
        public int? DocumentLookup1Id { get; set; }

        [Display(Name = "Document_DocumentLookup2")]
        public int? DocumentLookup2Id { get; set; }

        [Display(Name = "Document_DocumentLookup3")]
        public int? DocumentLookup3Id { get; set; }

        [Display(Name = "Document_DocumentText1")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string DocumentText1 { get; set; }

        [Display(Name = "Document_DocumentText2")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string DocumentText2 { get; set; }

        // END HIDDEN

        [Display(Name = "Memo")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string Memo { get; set; }

        [Display(Name = "Document_MemoIsCommon")]
        [DefaultValue(true)]
        public bool? MemoIsCommon { get; set; }

        [Display(Name = "Document_Agent")]
        public int? AgentId { get; set; }

        [ForeignKey(nameof(Line.DocumentId))]
        public List<TDocumentLine> Lines { get; set; }

        [Display(Name = "Document_Attachments")]
        [ForeignKey(nameof(Attachment.DocumentId))]
        public List<TAttachment> Attachments { get; set; }
    }

    public class DocumentForSave : DocumentForSave<LineForSave, AttachmentForSave>
    {

    }

    public class Document : DocumentForSave<Line, Attachment>
    {
        [Display(Name = "Definition")]
        public string DefinitionId { get; set; }

        [Display(Name = "Document_SerialNumber")]
        [AlwaysAccessible]
        public int? SerialNumber { get; set; }

        [Display(Name = "Document_State")]
        [AlwaysAccessible]
        [ChoiceList(new object[] {
            LineState.Draft,
            LineState.Void,
            LineState.Requested,
            LineState.Rejected,
            LineState.Authorized,
            LineState.Failed,
            LineState.Completed,
            LineState.Invalid,
            LineState.Finalized
        },
            new string[] {
            LineStateName.Draft,
            LineStateName.Void,
            LineStateName.Requested,
            LineStateName.Rejected,
            LineStateName.Authorized,
            LineStateName.Failed,
            LineStateName.Completed,
            LineStateName.Invalid,
            LineStateName.Finalized
        })]
        public short? State { get; set; }

        [Display(Name = "Document_PostingState")]
        [AlwaysAccessible]
        [ChoiceList(new object[] {
            DocState.Current,
            DocState.Posted,
            DocState.Canceled,
        },
            new string[] {
            DocStateName.Current,
            DocStateName.Posted,
            DocStateName.Canceled,
        })]
        public short? PostingState { get; set; }

        [Display(Name = "Document_PostingStateAt")]
        public DateTimeOffset? PostingStateAt { get; set; }

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
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For Query

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

        // HIDDEN

        [Display(Name = "Document_VoucherBooklet")]
        [ForeignKey(nameof(VoucherBookletId))]
        public VoucherBooklet VoucherBooklet { get; set; }

        [Display(Name = "Document_DocumentLookup1")]
        [ForeignKey(nameof(DocumentLookup1Id))]
        public Lookup DocumentLookup1 { get; set; }

        [Display(Name = "Document_DocumentLookup2")]
        [ForeignKey(nameof(DocumentLookup2Id))]
        public Lookup DocumentLookup2 { get; set; }

        [Display(Name = "Document_DocumentLookup3")]
        [ForeignKey(nameof(DocumentLookup3Id))]
        public Lookup DocumentLookup3 { get; set; }

        // END HIDDEN
    }

    public static class LineState
    {
        public const short Draft = 0;
        public const short Void = -1;
        public const short Requested = 1;
        public const short Rejected = -2;
        public const short Authorized = 2;
        public const short Failed = -3;
        public const short Completed = 3;
        public const short Invalid = -4;
        public const short Finalized = 4;
    }

    public static class LineStateName
    {
        private const string _prefix = "Document_State_";

        public const string Draft = _prefix + "0";
        public const string Void = _prefix + "minus_1";
        public const string Requested = _prefix + "1";
        public const string Rejected = _prefix + "minus_2";
        public const string Authorized = _prefix + "2";
        public const string Failed = _prefix + "minus_3";
        public const string Completed = _prefix + "3";
        public const string Invalid = _prefix + "minus_4";
        public const string Finalized = _prefix + "4";
    }

    public static class DocState
    {
        public const short Current = 0;
        public const short Posted = 0;
        public const short Canceled = 0;
    }

    public static class DocStateName
    {
        private const string _prefix = "Document_PostingState_";

        public const string Current = _prefix + "0";
        public const string Posted = _prefix + "1";
        public const string Canceled = _prefix + "minus_1";
    }
}
