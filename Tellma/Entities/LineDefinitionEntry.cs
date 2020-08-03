using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "LineDefinitionEntry", Plural = "LineDefinitionEntries")]
    public class LineDefinitionEntryForSave<TCustodyDef, TNotedRelationDef, TResourceDef> : EntityWithKey<int>
    {
        public short? Direction { get; set; }

        [Display(Name = "LineDefinitionEntry_AccountType")]
        public int? AccountTypeId { get; set; }

        [Display(Name = "LineDefinitionEntry_EntryType")]
        public int? EntryTypeId { get; set; }

        [ForeignKey(nameof(LineDefinitionEntryCustodyDefinition.LineDefinitionEntryId))]
        public List<TCustodyDef> CustodyDefinitions { get; set; }

        [ForeignKey(nameof(LineDefinitionEntryNotedRelationDefinition.LineDefinitionEntryId))]
        public List<TNotedRelationDef> NotedRelationDefinitions { get; set; }

        [ForeignKey(nameof(LineDefinitionEntryResourceDefinition.LineDefinitionEntryId))]
        public List<TResourceDef> ResourceDefinitions { get; set; }
    }

    public class LineDefinitionEntryForSave : LineDefinitionEntryForSave<LineDefinitionEntryCustodyDefinitionForSave, LineDefinitionEntryNotedRelationDefinitionForSave, LineDefinitionEntryResourceDefinitionForSave>
    {
    }

    public class LineDefinitionEntry : LineDefinitionEntryForSave<LineDefinitionEntryCustodyDefinition, LineDefinitionEntryNotedRelationDefinition, LineDefinitionEntryResourceDefinition>
    {
        [AlwaysAccessible]
        public int? Index { get; set; }

        public int? LineDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "LineDefinitionEntry_AccountType")]
        [ForeignKey(nameof(AccountTypeId))]
        public AccountType AccountType { get; set; }

        [Display(Name = "LineDefinitionEntry_EntryType")]
        [ForeignKey(nameof(EntryTypeId))]
        public EntryType EntryType { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
