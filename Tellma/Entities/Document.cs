using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using D = Tellma.Entities.Descriptors;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "Document", Plural = "Documents")]
    public class DocumentForSave<TDocumentLine, TAttachment> : EntityWithKey<int>
    {
        [Display(Name = "Document_SerialNumber")]
        [AlwaysAccessible]
        [UserKey]
        public int? SerialNumber { get; set; }

        [Display(Name = "Document_PostingDate")]
        public DateTime? PostingDate { get; set; }

        [IsCommonDisplay(Name = "Document_PostingDate")]
        public bool? PostingDateIsCommon { get; set; }

        [Display(Name = "Document_Clearance")]
        [ChoiceList(new object[] { (byte)0, (byte)1, (byte)2 },
            new string[] { "Document_Clearance_0", "Document_Clearance_1", "Document_Clearance_2" })]
        public byte? Clearance { get; set; }

        // HIDDEN

        [Display(Name = "Document_DocumentLookup1")]
        public int? DocumentLookup1Id { get; set; }

        [Display(Name = "Document_DocumentLookup2")]
        public int? DocumentLookup2Id { get; set; }

        [Display(Name = "Document_DocumentLookup3")]
        public int? DocumentLookup3Id { get; set; }

        [Display(Name = "Document_DocumentText1")]
        [StringLength(255)]
        public string DocumentText1 { get; set; }

        [Display(Name = "Document_DocumentText2")]
        [StringLength(255)]
        public string DocumentText2 { get; set; }

        // END HIDDEN

        [Display(Name = "Memo")]
        [StringLength(255)]
        public string Memo { get; set; }

        [IsCommonDisplay(Name = "Memo")]
        [DefaultValue(true)]
        public bool? MemoIsCommon { get; set; }

        [Display(Name = "Document_DebitContract")]
        public int? DebitContractId { get; set; }

        [IsCommonDisplay(Name = "Document_DebitContract")]
        public bool? DebitContractIsCommon { get; set; }

        [Display(Name = "Document_CreditContract")]
        public int? CreditContractId { get; set; }

        [IsCommonDisplay(Name = "Document_CreditContract")]
        public bool? CreditContractIsCommon { get; set; }

        [Display(Name = "Document_NotedContract")]
        public int? NotedContractId { get; set; }

        [IsCommonDisplay(Name = "Document_NotedContract")]
        public bool? NotedContractIsCommon { get; set; }

        [Display(Name = "Document_Segment")]
        public int? SegmentId { get; set; }

        [Display(Name = "Document_Time1")]
        public DateTime? Time1 { get; set; }

        [IsCommonDisplay(Name = "Document_Time1")]
        public bool? Time1IsCommon { get; set; }

        [Display(Name = "Document_Time2")]
        public DateTime? Time2 { get; set; }

        [IsCommonDisplay(Name = "Document_Time2")]
        public bool? Time2IsCommon { get; set; }

        [Display(Name = "Document_Quantity")]
        public decimal? Quantity { get; set; }

        [IsCommonDisplay(Name = "Document_Quantity")]
        public bool? QuantityIsCommon { get; set; }

        [Display(Name = "Document_Unit")]
        public int? UnitId { get; set; }

        [IsCommonDisplay(Name = "Document_Unit")]
        public bool? UnitIsCommon { get; set; }

        [Display(Name = "Document_Currency")]
        [StringLength(3)]
        public string CurrencyId { get; set; }

        [IsCommonDisplay(Name = "Document_Currency")]
        public bool? CurrencyIsCommon { get; set; }

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
        public int? DefinitionId { get; set; }

        [Display(Name = "Code")]
        [AlwaysAccessible]
        public string Code { get; set; }

        [Display(Name = "Document_State")]
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
        public short? State { get; set; }

        [Display(Name = "Document_StateAt")]
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
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For Query
        [Display(Name = "Document_DebitContract")]
        [ForeignKey(nameof(DebitContractId))]
        public Contract DebitContract { get; set; }

        [Display(Name = "Document_CreditContract")]
        [ForeignKey(nameof(CreditContractId))]
        public Contract CreditContract { get; set; }

        [Display(Name = "Document_NotedContract")]
        [ForeignKey(nameof(NotedContractId))]
        public Contract NotedContract { get; set; }

        [Display(Name = "Document_Segment")]
        [ForeignKey(nameof(SegmentId))]
        public Center Segment { get; set; }

        [Display(Name = "Document_Unit")]
        [ForeignKey(nameof(UnitId))]
        public Unit Unit { get; set; }

        [Display(Name = "Document_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

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

        [Display(Name = "Definition")]
        [ForeignKey(nameof(DefinitionId))]
        public DocumentDefinition Definition { get; set; }

        // HIDDEN

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
        public const short Current = 0;
        public const short Posted = 0;
        public const short Canceled = 0;
    }

    public static class DocStateName
    {
        private const string _prefix = "Document_State_";

        public const string Current = _prefix + "0";
        public const string Posted = _prefix + "1";
        public const string Canceled = _prefix + "minus_1";
    }

    public static class DocDetails
    {
        // ------------------------------------------------
        // Paths to return on the level of each entity type
        // IMPORTANT: Keep in sync with ApplicationRepository.GetDocumentsByIds
        // ------------------------------------------------

        public static IEnumerable<string> DocumentPaths() => DocumentProps
            .Concat(LinePaths(nameof(Document.Lines)))
            .Concat(AttachmentPaths(nameof(Document.Attachments)))
            .Concat(DocumentStateChangePaths(nameof(Document.StatesHistory)))
            .Concat(DocumentAssignmentPaths(nameof(Document.AssignmentsHistory)))
            .Concat(ContractPaths(nameof(Document.DebitContract)))
            .Concat(ContractPaths(nameof(Document.CreditContract)))
            .Concat(ContractPaths(nameof(Document.NotedContract)))
            .Concat(CenterPaths(nameof(Document.Segment)))
            .Concat(UnitPaths(nameof(Document.Unit)))
            .Concat(CurrencyPaths(nameof(Document.Currency)))
            .Concat(LookupPaths(nameof(Document.DocumentLookup1)))
            .Concat(LookupPaths(nameof(Document.DocumentLookup2)))
            .Concat(LookupPaths(nameof(Document.DocumentLookup3)))
            .Concat(UserPaths(nameof(Document.CreatedBy)))
            .Concat(UserPaths(nameof(Document.ModifiedBy)))
            .Concat(UserPaths(nameof(Document.Assignee)));
        public static IEnumerable<string> LinePaths(string path = null) => LineProps
            .Concat(EntryPaths(nameof(Line.Entries)))
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> EntryPaths(string path = null) => EntryProps
            .Concat(AccountPaths(nameof(Entry.Account)))
            .Concat(CurrencyPaths(nameof(Entry.Currency)))
            .Concat(EntryResourcePaths(nameof(Entry.Resource)))
            .Concat(EntryContractPaths(nameof(Entry.Contract)))
            .Concat(EntryTypePaths(nameof(Entry.EntryType)))
            .Concat(ContractPaths(nameof(Entry.NotedContract)))
            .Concat(CenterPaths(nameof(Entry.Center)))
            .Concat(UnitPaths(nameof(Entry.Unit)))
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> AttachmentPaths(string path = null) => AttachmentProps
            .Concat(UserPaths(nameof(Attachment.CreatedBy)))
            .Concat(UserPaths(nameof(Attachment.ModifiedBy)))
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> DocumentStateChangePaths(string path = null) => DocumentStateChangeProps
            .Concat(UserPaths(nameof(DocumentStateChange.ModifiedBy)))
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> DocumentAssignmentPaths(string path = null) => DocumentAssignmentProps
            .Concat(UserPaths(nameof(DocumentAssignment.CreatedBy)))
            .Concat(UserPaths(nameof(DocumentAssignment.Assignee)))
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> EntryContractPaths(string path = null) => ContractPaths(path)
            .Concat( // Entry Contract also adds the Currency and Center
                CurrencyPaths(nameof(Contract.Currency)).Select(p => path == null ? p : $"{path}/{p}")
            ).Concat(
                CenterPaths(nameof(Contract.Center)).Select(p => path == null ? p : $"{path}/{p}")
            );
        public static IEnumerable<string> ContractPaths(string path = null) => ContractProps
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> EntryResourcePaths(string path = null) => ResourcePaths(path)
            .Concat( // Entry Resource also adds the Currency and Center
                CurrencyPaths(nameof(Resource.Currency)).Select(p => path == null ? p : $"{path}/{p}")
            ).Concat(
                CenterPaths(nameof(Resource.Center)).Select(p => path == null ? p : $"{path}/{p}")
            );
        public static IEnumerable<string> ResourcePaths(string path = null) => ResourceProps
            // This is used in account, it does not need currency or center, since they already come with the account
            .Concat(UnitPaths(nameof(Resource.Unit)))
            .Concat(ResourceUnitPaths(nameof(Resource.Units)))
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> ResourceUnitPaths(string path = null) => ResourceUnitsProps
            .Concat(UnitPaths(nameof(ResourceUnit.Unit)))
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> CenterPaths(string path = null) => CenterProps
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> UnitPaths(string path = null) => UnitProps
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> CurrencyPaths(string path = null) => CurrencyProps
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> UserPaths(string path = null) => UserProps
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> LookupPaths(string path = null) => LookupProps
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> EntryTypePaths(string path = null) => EntryTypeProps
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> EntryTypeParentPaths(string path = null) => EntryTypeParentProps
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> AccountPaths(string path = null) => AccountProps
            .Concat(AccountTypePaths(nameof(Account.AccountType)))
            .Concat(CenterPaths(nameof(Account.Center)))
            .Concat(EntryTypePaths(nameof(Account.EntryType)))
            .Concat(CurrencyPaths(nameof(Account.Currency)))
            .Concat(ContractPaths(nameof(Account.Contract)))
            .Concat(ResourcePaths(nameof(Account.Resource)))
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> AccountTypePaths(string path = null) => AccountTypeProps
            .Concat(EntryTypeParentPaths(nameof(AccountType.EntryTypeParent)))
            .Select(p => path == null ? p : $"{path}/{p}");

        // -------------------------------------------------------------
        // Simple properties to include on the level of each entity type
        // -------------------------------------------------------------

        public static IEnumerable<string> DocumentProps => D.TypeDescriptor.Get<Document>().SimpleProperties.Select(p => p.Name);
        public static IEnumerable<string> LineProps => D.TypeDescriptor.Get<Line>().SimpleProperties.Select(p => p.Name);
        public static IEnumerable<string> EntryProps => D.TypeDescriptor.Get<Entry>().SimpleProperties.Select(p => p.Name);
        public static IEnumerable<string> AttachmentProps => D.TypeDescriptor.Get<Attachment>().SimpleProperties.Select(p => p.Name);
        public static IEnumerable<string> DocumentAssignmentProps => D.TypeDescriptor.Get<DocumentAssignment>().SimpleProperties.Select(p => p.Name);
        public static IEnumerable<string> DocumentStateChangeProps => D.TypeDescriptor.Get<DocumentStateChange>().SimpleProperties.Select(p => p.Name);
        public static IEnumerable<string> UnitProps => Enum(nameof(Unit.Name), nameof(Unit.Name2), nameof(Unit.Name3));
        public static IEnumerable<string> CurrencyProps => Enum(nameof(Currency.Name), nameof(Currency.Name2), nameof(Currency.Name3), nameof(Currency.E));
        public static IEnumerable<string> UserProps => Enum(nameof(User.Name), nameof(User.Name2), nameof(User.Name3), nameof(User.ImageId));
        public static IEnumerable<string> ResourceProps => Enum(nameof(Resource.Name), nameof(Resource.Name2), nameof(Resource.Name3), nameof(Resource.DefinitionId));
        public static IEnumerable<string> ResourceUnitsProps => Enum();
        public static IEnumerable<string> LookupProps => Enum(nameof(Lookup.Name), nameof(Lookup.Name2), nameof(Lookup.Name3), nameof(Lookup.DefinitionId));
        public static IEnumerable<string> ContractProps => Enum(nameof(Contract.Name), nameof(Contract.Name2), nameof(Contract.Name3), nameof(Contract.DefinitionId));
        public static IEnumerable<string> CenterProps => Enum(nameof(Center.Name), nameof(Center.Name2), nameof(Center.Name3));
        public static IEnumerable<string> AccountProps => Enum(
            // Names
            nameof(Account.Name),
            nameof(Account.Name2),
            nameof(Account.Name3),
            nameof(Account.Code),

            // Definitions
            nameof(Account.ContractDefinitionId),
            nameof(Account.NotedContractDefinitionId),
            nameof(Account.ResourceDefinitionId)
        );
        public static IEnumerable<string> EntryTypeProps => Enum(nameof(EntryType.Name), nameof(EntryType.Name2), nameof(EntryType.Name3), nameof(EntryType.IsActive));
        public static IEnumerable<string> EntryTypeParentProps => Enum(nameof(EntryType.IsActive));
        public static IEnumerable<string> AccountTypeProps => Enum(
            // Names
            nameof(AccountType.Name),
            nameof(AccountType.Name2),
            nameof(AccountType.Name3),

            // Misc
            nameof(AccountType.EntryTypeParentId),

            // Labels
            nameof(AccountType.DueDateLabel), nameof(AccountType.DueDateLabel2), nameof(AccountType.DueDateLabel3),
            nameof(AccountType.Time1Label), nameof(AccountType.Time1Label2), nameof(AccountType.Time1Label3),
            nameof(AccountType.Time2Label), nameof(AccountType.Time2Label2), nameof(AccountType.Time2Label3),
            nameof(AccountType.ExternalReferenceLabel), nameof(AccountType.ExternalReferenceLabel2), nameof(AccountType.ExternalReferenceLabel3),
            nameof(AccountType.AdditionalReferenceLabel), nameof(AccountType.AdditionalReferenceLabel2), nameof(AccountType.AdditionalReferenceLabel3),
            nameof(AccountType.NotedAgentNameLabel), nameof(AccountType.NotedAgentNameLabel2), nameof(AccountType.NotedAgentNameLabel3),
            nameof(AccountType.NotedAmountLabel), nameof(AccountType.NotedAmountLabel2), nameof(AccountType.NotedAmountLabel3),
            nameof(AccountType.NotedDateLabel), nameof(AccountType.NotedDateLabel2), nameof(AccountType.NotedDateLabel3)
         );

        // Helper method
        private static IEnumerable<string> Enum(params string[] ps)
        {
            foreach (var p in ps)
            {
                yield return p;
            }
        }
    }
}
