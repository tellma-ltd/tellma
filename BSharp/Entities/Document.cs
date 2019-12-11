using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
{
    [StrongEntity]
    public class DocumentForSave<TDocumentLine> : EntityWithKey<int>
    {
        [Display(Name = "OperatingSegment")]
        public int? OperatingSegmentId { get; set; }

        [Display(Name = "Document_DocumentDate")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        public DateTime? DocumentDate { get; set; }

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

        [ForeignKey(nameof(Line.DocumentId))]
        public List<TDocumentLine> Lines { get; set; }
    }

    public class DocumentForSave : DocumentForSave<LineForSave>
    {

    }

    public class Document : DocumentForSave<Line>
    {
        [Display(Name = "Definition")]
        public string DefinitionId { get; set; }

        [Display(Name = "Document_SerialNumber")]
        [AlwaysAccessible]
        public int? SerialNumber { get; set; }

        [Display(Name = "Document_State")]
        [AlwaysAccessible]
        [ChoiceList(new object[] {
            DocState.Draft,
            DocState.Void,
            DocState.Requested,
            DocState.Rejected,
            DocState.Authorized,
            DocState.Failed,
            DocState.Completed,
            DocState.Invalid,
            DocState.Reviewed,
            DocState.Closed
        },
            new string[] {
            DocStateName.Draft,
            DocStateName.Void,
            DocStateName.Requested,
            DocStateName.Rejected,
            DocStateName.Authorized,
            DocStateName.Failed,
            DocStateName.Completed,
            DocStateName.Invalid,
            DocStateName.Reviewed,
            DocStateName.Closed
        })]
        public int? State { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "OperatingSegment")]
        [ForeignKey(nameof(OperatingSegmentId))]
        public ResponsibilityCenter OperatingSegment { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }

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

    public static class DocState
    {
        public const int Draft = 0;
        public const int Void = -1;
        public const int Requested = 1;
        public const int Rejected = -2;
        public const int Authorized = 2;
        public const int Failed = -3;
        public const int Completed = 3;
        public const int Invalid = -4;
        public const int Reviewed = 4;
        public const int Closed = 5;
    }

    public static class DocStateName
    {
        private const string _prefix = "Document_State_";

        public const string Draft = _prefix + nameof(Draft);
        public const string Void = _prefix + nameof(Void);
        public const string Requested = _prefix + nameof(Requested);
        public const string Rejected = _prefix + nameof(Rejected);
        public const string Authorized = _prefix + nameof(Authorized);
        public const string Failed = _prefix + nameof(Failed);
        public const string Completed = _prefix + nameof(Completed);
        public const string Invalid = _prefix + nameof(Invalid);
        public const string Reviewed = _prefix + nameof(Reviewed);
        public const string Closed = _prefix + nameof(Closed);
    }
}
