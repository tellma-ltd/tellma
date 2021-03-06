﻿using System;
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
    public class DocumentForSave<TDocumentLine, TLineDefinitionEntry, TAttachment> : EntityWithKey<int>
    {
        [Display(Name = "Document_SerialNumber")]
        [AlwaysAccessible]
        [NotNull]
        [UserKey]
        public int? SerialNumber { get; set; }

        [Display(Name = "Document_Clearance")]
        [NotNull]
        [ChoiceList(new object[] { (byte)0, (byte)1, (byte)2 },
            new string[] { "Document_Clearance_0", "Document_Clearance_1", "Document_Clearance_2" })]
        public byte? Clearance { get; set; }

        [Display(Name = "Document_PostingDate")]
        public DateTime? PostingDate { get; set; }

        [IsCommonDisplay(Name = "Document_PostingDate")]
        [NotNull]
        public bool? PostingDateIsCommon { get; set; }

        [Display(Name = "Memo")]
        [StringLength(255)]
        public string Memo { get; set; }

        [IsCommonDisplay(Name = "Memo")]
        [NotNull]
        [DefaultValue(true)]
        public bool? MemoIsCommon { get; set; }

        [Display(Name = "Entry_Currency")]
        [StringLength(3)]
        public string CurrencyId { get; set; }

        [IsCommonDisplay(Name = "Entry_Currency")]
        [NotNull]
        public bool? CurrencyIsCommon { get; set; }

        [Display(Name = "Document_Center")]
        public int? CenterId { get; set; }

        [IsCommonDisplay(Name = "Document_Center")]
        [NotNull]
        public bool? CenterIsCommon { get; set; }

        [Display(Name = "Entry_Custodian")]
        public int? CustodianId { get; set; }

        [IsCommonDisplay(Name = "Entry_Custodian")]
        [NotNull]
        public bool? CustodianIsCommon { get; set; }

        [Display(Name = "Entry_Custody")]
        public int? CustodyId { get; set; }

        [IsCommonDisplay(Name = "Entry_Custody")]
        [NotNull]
        public bool? CustodyIsCommon { get; set; }

        [Display(Name = "Entry_Participant")]
        public int? ParticipantId { get; set; }

        [IsCommonDisplay(Name = "Entry_Participant")]
        [NotNull]
        public bool? ParticipantIsCommon { get; set; }

        [Display(Name = "Entryt_Resource")]
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

        [Display(Name = "Entry_InternalReference")]
        [StringLength(50)]
        public string InternalReference { get; set; }

        [IsCommonDisplay(Name = "Entry_InternalReference")]
        [NotNull]
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
        [NotNull]
        public int? DefinitionId { get; set; }

        [Display(Name = "Code")]
        [NotNull]
        [AlwaysAccessible]
        public string Code { get; set; }

        [Display(Name = "Document_State")]
        [AlwaysAccessible]
        [NotNull]
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
        [NotNull]
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

        [Display(Name = "Entry_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "Document_Center")]
        [ForeignKey(nameof(CenterId))]
        public Center Center { get; set; }

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
        // ------------------------------------------------
        public static IEnumerable<string> DocumentPaths() => DocumentProps

            // Weak Collections
            .Concat(LineDefinitionEntryPaths(nameof(Document.LineDefinitionEntries)))
            .Concat(LinePaths(nameof(Document.Lines)))
            .Concat(AttachmentPaths(nameof(Document.Attachments)))
            .Concat(DocumentStateChangePaths(nameof(Document.StatesHistory)))
            .Concat(DocumentAssignmentPaths(nameof(Document.AssignmentsHistory)))

            // Navigation Properties
            .Concat(CurrencyPaths(nameof(Document.Currency)))
            .Concat(CenterPaths(nameof(Document.Center)))

            .Concat(RelationPaths(nameof(Document.Custodian)))
            .Concat(CustodyPaths(nameof(Document.Custody)))
            .Concat(RelationPaths(nameof(Document.Participant)))
            .Concat(ResourcePaths(nameof(Document.Resource)))

            .Concat(UnitPaths(nameof(Document.Unit)))

            .Concat(UserPaths(nameof(Document.CreatedBy)))
            .Concat(UserPaths(nameof(Document.ModifiedBy)))
            .Concat(UserPaths(nameof(Document.Assignee)));

        public static IEnumerable<string> LineDefinitionEntryPaths(string path = null) => LineDefinitionEntryProps
            .Concat(CurrencyPaths(nameof(DocumentLineDefinitionEntry.Currency)))
            .Concat(CenterPaths(nameof(DocumentLineDefinitionEntry.Center)))

            .Concat(RelationPaths(nameof(DocumentLineDefinitionEntry.Custodian)))
            .Concat(CustodyPaths(nameof(DocumentLineDefinitionEntry.Custody)))
            .Concat(RelationPaths(nameof(DocumentLineDefinitionEntry.Participant)))
            .Concat(ResourcePaths(nameof(DocumentLineDefinitionEntry.Resource)))

            .Concat(UnitPaths(nameof(DocumentLineDefinitionEntry.Unit)))
            .Select(p => path == null ? p : $"{path}.{p}");

        public static IEnumerable<string> LinePaths(string path = null) => LineProps
            .Concat(EntryPaths(nameof(Line.Entries)))
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> EntryPaths(string path = null) => EntryProps
            .Concat(AccountPaths(nameof(Entry.Account)))
            .Concat(CurrencyPaths(nameof(Entry.Currency)))
            .Concat(RelationPaths(nameof(Entry.Custodian)))
            .Concat(EntryCustodyPaths(nameof(Entry.Custody)))
            .Concat(RelationPaths(nameof(Entry.Participant)))
            .Concat(EntryResourcePaths(nameof(Entry.Resource)))
            .Concat(EntryTypePaths(nameof(Entry.EntryType)))
            .Concat(CenterPaths(nameof(Entry.Center)))
            .Concat(UnitPaths(nameof(Entry.Unit)))
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> AttachmentPaths(string path = null) => AttachmentProps
            .Concat(UserPaths(nameof(Attachment.CreatedBy)))
            .Concat(UserPaths(nameof(Attachment.ModifiedBy)))
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> DocumentStateChangePaths(string path = null) => DocumentStateChangeProps
            .Concat(UserPaths(nameof(DocumentStateChange.ModifiedBy)))
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> DocumentAssignmentPaths(string path = null) => DocumentAssignmentProps
            .Concat(UserPaths(nameof(DocumentAssignment.CreatedBy)))
            .Concat(UserPaths(nameof(DocumentAssignment.Assignee)))
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> CustodyPaths(string path = null) => CustodyProps
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> AccountCustodyPaths(string path = null) => CustodyPaths(path)
            // This is used in account, it does not need currency or center, since they already come with the account
            .Concat(RelationPaths(nameof(Custody.Custodian)).Select(p => path == null ? p : $"{path}.{p}"));
        public static IEnumerable<string> EntryCustodyPaths(string path = null) => AccountCustodyPaths(path)
            // Entry Custody also adds the Custodian, Currency and Center
            .Concat(CurrencyPaths(nameof(Custody.Currency)).Select(p => path == null ? p : $"{path}.{p}"))
            .Concat(CenterPaths(nameof(Custody.Center)).Select(p => path == null ? p : $"{path}.{p}"));
        public static IEnumerable<string> RelationPaths(string path = null) => RelationProps
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> ResourcePaths(string path = null) => ResourceProps
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> AccountResourcePaths(string path = null) => ResourcePaths(path)
            // This is used in account, it does not need currency or center, since they already come with the account
            .Concat(RelationPaths(nameof(Resource.Participant)).Select(p => path == null ? p : $"{path}.{p}"))
            .Concat(UnitPaths(nameof(Resource.Unit)).Select(p => path == null ? p : $"{path}.{p}"))
            .Concat(ResourceUnitPaths(nameof(Resource.Units)).Select(p => path == null ? p : $"{path}.{p}"));
        public static IEnumerable<string> EntryResourcePaths(string path = null) => AccountResourcePaths(path)
            // Entry Resource also adds the Currency, Center, cost center and participant
            .Concat(CurrencyPaths(nameof(Resource.Currency)).Select(p => path == null ? p : $"{path}.{p}"))
            .Concat(CenterPaths(nameof(Resource.Center)).Select(p => path == null ? p : $"{path}.{p}"))
            .Concat(CenterPaths(nameof(Resource.CostCenter)).Select(p => path == null ? p : $"{path}.{p}"));
        public static IEnumerable<string> ResourceUnitPaths(string path = null) => ResourceUnitsProps
            .Concat(UnitPaths(nameof(ResourceUnit.Unit)))
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> CenterPaths(string path = null) => CenterProps
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> UnitPaths(string path = null) => UnitProps
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> CurrencyPaths(string path = null) => CurrencyProps
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> UserPaths(string path = null) => UserProps
            .Select(p => path == null ? p : $"{path}.{p}");
        //public static IEnumerable<string> LookupPaths(string path = null) => LookupProps
        //    .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> EntryTypePaths(string path = null) => EntryTypeProps
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> EntryTypeParentPaths(string path = null) => EntryTypeParentProps
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> AccountPaths(string path = null) => AccountProps
            .Concat(AccountTypePaths(nameof(Account.AccountType)))
            .Concat(CenterPaths(nameof(Account.Center)))
            .Concat(EntryTypePaths(nameof(Account.EntryType)))
            .Concat(CurrencyPaths(nameof(Account.Currency)))
            .Concat(CustodyPaths(nameof(Account.Custody)))
            .Concat(AccountResourcePaths(nameof(Account.Resource)))
            .Concat(RelationPaths(nameof(Account.Participant)))
            .Concat(RelationPaths(nameof(Account.Custodian)))
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> AccountTypePaths(string path = null) => AccountTypeProps
            .Concat(EntryTypeParentPaths(nameof(AccountType.EntryTypeParent)))
            .Select(p => path == null ? p : $"{path}.{p}");

        // -------------------------------------------------------------
        // Simple properties to include on the level of each entity type
        // -------------------------------------------------------------

        public static IEnumerable<string> DocumentProps => D.TypeDescriptor.Get<Document>().SimpleProperties.Select(p => p.Name);
        public static IEnumerable<string> LineDefinitionEntryProps => D.TypeDescriptor.Get<DocumentLineDefinitionEntry>().SimpleProperties.Select(p => p.Name);
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
        //public static IEnumerable<string> LookupProps => Enum(nameof(Lookup.Name), nameof(Lookup.Name2), nameof(Lookup.Name3), nameof(Lookup.DefinitionId));
        public static IEnumerable<string> CustodyProps => Enum(nameof(Custody.Name), nameof(Custody.Name2), nameof(Custody.Name3), nameof(Custody.DefinitionId));
        public static IEnumerable<string> RelationProps => Enum(nameof(Relation.Name), nameof(Relation.Name2), nameof(Relation.Name3), nameof(Relation.DefinitionId));
        public static IEnumerable<string> CenterProps => Enum(nameof(Center.Name), nameof(Center.Name2), nameof(Center.Name3));
        public static IEnumerable<string> AccountProps => Enum(
            // Names
            nameof(Account.Name),
            nameof(Account.Name2),
            nameof(Account.Name3),
            nameof(Account.Code),

            // Misc.
            nameof(Account.IsBusinessUnit),

            // Definitions
            nameof(Account.CustodyDefinitionId),
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
            nameof(AccountType.StandardAndPure),
            nameof(AccountType.IsBusinessUnit),

            // Definitions
            nameof(AccountType.CustodianDefinitionId),
            nameof(AccountType.ParticipantDefinitionId),

            // Labels
            nameof(AccountType.Time1Label), nameof(AccountType.Time1Label2), nameof(AccountType.Time1Label3),
            nameof(AccountType.Time2Label), nameof(AccountType.Time2Label2), nameof(AccountType.Time2Label3),
            nameof(AccountType.ExternalReferenceLabel), nameof(AccountType.ExternalReferenceLabel2), nameof(AccountType.ExternalReferenceLabel3),
            nameof(AccountType.InternalReferenceLabel), nameof(AccountType.InternalReferenceLabel2), nameof(AccountType.InternalReferenceLabel3),
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
