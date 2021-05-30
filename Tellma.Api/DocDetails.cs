using System.Collections.Generic;
using System.Linq;
using Tellma.Model.Common;
using Tellma.Model.Application;

namespace Tellma.Api
{
    /// <summary>
    /// Request URLs for the <see cref="Document"/> API may end up cumbersome because of the
    /// large number of columns contained in the select argument. So as an optimization we allow
    /// special shorthand notation in the select argument that is translated on the server side
    /// to a large select.
    /// </summary>
    internal static class DocDetails
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
            .Concat(RelationPaths(nameof(Document.Relation)))
            .Concat(ResourcePaths(nameof(Document.Resource)))
            .Concat(RelationPaths(nameof(Document.NotedRelation)))
            .Concat(RelationPaths(nameof(Document.ReferenceSource)))
            .Concat(UnitPaths(nameof(Document.Unit)))
            .Concat(UnitPaths(nameof(Document.DurationUnit)))
            .Concat(UserPaths(nameof(Document.CreatedBy)))
            .Concat(UserPaths(nameof(Document.ModifiedBy)))
            .Concat(UserPaths(nameof(Document.Assignee)));
        public static IEnumerable<string> LineDefinitionEntryPaths(string path = null) => LineDefinitionEntryProps
            .Concat(CurrencyPaths(nameof(DocumentLineDefinitionEntry.Currency)))
            .Concat(CenterPaths(nameof(DocumentLineDefinitionEntry.Center)))
            .Concat(RelationPaths(nameof(DocumentLineDefinitionEntry.Custodian)))
            .Concat(RelationPaths(nameof(DocumentLineDefinitionEntry.Relation)))
            .Concat(ResourcePaths(nameof(DocumentLineDefinitionEntry.Resource)))
            .Concat(RelationPaths(nameof(DocumentLineDefinitionEntry.NotedRelation)))
            .Concat(RelationPaths(nameof(DocumentLineDefinitionEntry.ReferenceSource)))
            .Concat(UnitPaths(nameof(DocumentLineDefinitionEntry.Unit)))
            .Concat(UnitPaths(nameof(DocumentLineDefinitionEntry.DurationUnit)))
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> LinePaths(string path = null) => LineProps
            .Concat(EntryPaths(nameof(Line.Entries)))
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> EntryPaths(string path = null) => EntryProps
            .Concat(AccountPaths(nameof(Entry.Account)))
            .Concat(CurrencyPaths(nameof(Entry.Currency)))
            .Concat(RelationPaths(nameof(Entry.Custodian)))
            .Concat(EntryRelationPaths(nameof(Entry.Relation)))
            .Concat(EntryResourcePaths(nameof(Entry.Resource)))
            .Concat(EntryRelationPaths(nameof(Entry.NotedRelation)))
            .Concat(RelationPaths(nameof(Entry.ReferenceSource)))
            .Concat(EntryTypePaths(nameof(Entry.EntryType)))
            .Concat(CenterPaths(nameof(Entry.Center)))
            .Concat(UnitPaths(nameof(Entry.Unit)))
            .Concat(UnitPaths(nameof(Entry.DurationUnit)))
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
        public static IEnumerable<string> RelationPaths(string path = null) => RelationProps
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> EntryRelationPaths(string path = null) => RelationPaths(path)
            // Entry Relation also adds the Currency, Center, cost center and participant
            .Concat(CurrencyPaths(nameof(Relation.Currency)).Select(p => path == null ? p : $"{path}.{p}"))
            .Concat(CenterPaths(nameof(Relation.Center)).Select(p => path == null ? p : $"{path}.{p}"));
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
            .Concat(CenterPaths(nameof(Resource.Center)).Select(p => path == null ? p : $"{path}.{p}"));
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
        public static IEnumerable<string> EntryTypePaths(string path = null) => EntryTypeProps
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> EntryTypeParentPaths(string path = null) => EntryTypeParentProps
            .Select(p => path == null ? p : $"{path}.{p}");
        public static IEnumerable<string> AccountPaths(string path = null) => AccountProps
            .Concat(AccountTypePaths(nameof(Account.AccountType)))
            .Concat(CenterPaths(nameof(Account.Center)))
            .Concat(EntryTypePaths(nameof(Account.EntryType)))
            .Concat(CurrencyPaths(nameof(Account.Currency)))
            .Concat(RelationPaths(nameof(Account.Custodian)))
            .Concat(RelationPaths(nameof(Account.Relation)))
            .Concat(AccountResourcePaths(nameof(Account.Resource)))
            .Concat(RelationPaths(nameof(Account.NotedRelation)))
            .Select(p => path == null ? p : $"{path}.{p}");

        public static IEnumerable<string> AccountTypePaths(string path = null) => AccountTypeProps
            .Concat(EntryTypeParentPaths(nameof(AccountType.EntryTypeParent)))
            .Select(p => path == null ? p : $"{path}.{p}");

        // -------------------------------------------------------------
        // Simple properties to include on the level of each entity type
        // -------------------------------------------------------------

        public static IEnumerable<string> DocumentProps => TypeDescriptor.Get<Document>().SimpleProperties.Select(p => p.Name);
        public static IEnumerable<string> LineDefinitionEntryProps => TypeDescriptor.Get<DocumentLineDefinitionEntry>().SimpleProperties.Select(p => p.Name);
        public static IEnumerable<string> LineProps => TypeDescriptor.Get<Line>().SimpleProperties.Select(p => p.Name);
        public static IEnumerable<string> EntryProps => TypeDescriptor.Get<Entry>().SimpleProperties.Select(p => p.Name);
        public static IEnumerable<string> AttachmentProps => TypeDescriptor.Get<Attachment>().SimpleProperties.Select(p => p.Name);
        public static IEnumerable<string> DocumentAssignmentProps => TypeDescriptor.Get<DocumentAssignment>().SimpleProperties.Select(p => p.Name);
        public static IEnumerable<string> DocumentStateChangeProps => TypeDescriptor.Get<DocumentStateChange>().SimpleProperties.Select(p => p.Name);
        public static IEnumerable<string> UnitProps => Enum(nameof(Unit.Name), nameof(Unit.Name2), nameof(Unit.Name3));
        public static IEnumerable<string> CurrencyProps => Enum(nameof(Currency.Name), nameof(Currency.Name2), nameof(Currency.Name3), nameof(Currency.E));
        public static IEnumerable<string> UserProps => Enum(nameof(User.Name), nameof(User.Name2), nameof(User.Name3), nameof(User.ImageId));
        public static IEnumerable<string> ResourceProps => Enum(nameof(Resource.Name), nameof(Resource.Name2), nameof(Resource.Name3), nameof(Resource.DefinitionId));
        public static IEnumerable<string> ResourceUnitsProps => Enum();
        public static IEnumerable<string> RelationProps => Enum(nameof(Relation.Name), nameof(Relation.Name2), nameof(Relation.Name3), nameof(Relation.DefinitionId));
        public static IEnumerable<string> CenterProps => Enum(nameof(Center.Name), nameof(Center.Name2), nameof(Center.Name3));
        public static IEnumerable<string> AccountProps => Enum(
            // Names
            nameof(Account.Name),
            nameof(Account.Name2),
            nameof(Account.Name3),
            nameof(Account.Code),

            // Definitions
            nameof(Account.RelationDefinitionId),
            nameof(Account.ResourceDefinitionId),
            nameof(Account.NotedRelationDefinitionId)
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

            // Definitions
            nameof(AccountType.CustodianDefinitionId),

            // Labels
            nameof(AccountType.Time1Label), nameof(AccountType.Time1Label2), nameof(AccountType.Time1Label3),
            nameof(AccountType.Time2Label), nameof(AccountType.Time2Label2), nameof(AccountType.Time2Label3),
            nameof(AccountType.ExternalReferenceLabel), nameof(AccountType.ExternalReferenceLabel2), nameof(AccountType.ExternalReferenceLabel3),
            nameof(AccountType.ReferenceSourceLabel), nameof(AccountType.ReferenceSourceLabel2), nameof(AccountType.ReferenceSourceLabel3),
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
