using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
{
    [StrongEntity]
    public class DocumentForSave<TDocumentLine> : EntityWithKey<int>
    {
        [Display(Name = "Document_DocumentDate")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        public DateTime? DocumentDate { get; set; }

        [Display(Name = "Document_VoucherBooklet")]
        public int? VoucherBookletId { get; set; }

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

        [Display(Name = "Memo")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string Memo { get; set; }

        [Display(Name = "Document_MemoIsCommon")]
        public bool? MemoIsCommon { get; set; }

        public List<TDocumentLine> Lines { get; set; }
    }

    public class DocumentForSave : DocumentForSave<DocumentLineForSave>
    {

    }

    public class Document : DocumentForSave<DocumentLine>
    {
        public string DocumentDefinitionId { get; set; }

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
            DocState.Posted
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
            DocStateName.Posted
        })]
        public string State { get; set; }
        public decimal? SortKey { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "Document_VoucherBooklet")]
        [ForeignKey(nameof(VoucherBookletId))]
        public VoucherBooklet VoucherBooklet { get; set; }

        [Display(Name = "Document_DocumentLookup1")]
        [ForeignKey(nameof(DocumentLookup1Id))]
        public ResourceLookup DocumentLookup1 { get; set; }

        [Display(Name = "Document_DocumentLookup2")]
        [ForeignKey(nameof(DocumentLookup2Id))]
        public ResourceLookup DocumentLookup2 { get; set; }

        [Display(Name = "Document_DocumentLookup3")]
        [ForeignKey(nameof(DocumentLookup3Id))]
        public ResourceLookup DocumentLookup3 { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }

    public static class DocState
    {
        public const string Draft = nameof(Draft);
        public const string Void = nameof(Void);
        public const string Requested = nameof(Requested);
        public const string Rejected = nameof(Rejected);
        public const string Authorized = nameof(Authorized);
        public const string Failed = nameof(Failed);
        public const string Completed = nameof(Completed);
        public const string Invalid = nameof(Invalid);
        public const string Posted = nameof(Posted);
    }

    public static class DocStateName
    {
        private const string _prefix = "Document_State_";

        public const string Draft = _prefix + nameof(Draft);
        public const string Void =  _prefix + nameof(Void);
        public const string Requested =  _prefix + nameof(Requested);
        public const string Rejected =  _prefix + nameof(Rejected);
        public const string Authorized =  _prefix + nameof(Authorized);
        public const string Failed =  _prefix + nameof(Failed);
        public const string Completed =  _prefix + nameof(Completed);
        public const string Invalid =  _prefix + nameof(Invalid);
        public const string Posted =  _prefix + nameof(Posted);
    }
}
