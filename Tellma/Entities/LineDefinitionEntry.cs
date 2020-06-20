using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "LineDefinitionEntry", Plural = "LineDefinitionEntries")]
    public class LineDefinitionEntryForSave<TContractDef, TNotedContractDef, TResourceDef> : EntityWithKey<int>
    {
        public short? Direction { get; set; }

        [Display(Name = "LineDefinitionEntry_AccountType")]
        public int? AccountTypeId { get; set; }

        [Display(Name = "LineDefinitionEntry_EntryType")]
        public int? EntryTypeId { get; set; }

        [ForeignKey(nameof(LineDefinitionEntryContractDefinition.LineDefinitionEntryId))]
        public List<TContractDef> ContractDefinitions { get; set; }

        [ForeignKey(nameof(LineDefinitionEntryNotedContractDefinition.LineDefinitionEntryId))]
        public List<TNotedContractDef> NotedContractDefinitions { get; set; }

        [ForeignKey(nameof(LineDefinitionEntryResourceDefinition.LineDefinitionEntryId))]
        public List<TResourceDef> ResourceDefinitions { get; set; }
    }

    public class LineDefinitionEntryForSave : LineDefinitionEntryForSave<LineDefinitionEntryContractDefinitionForSave, LineDefinitionEntryNotedContractDefinitionForSave, LineDefinitionEntryResourceDefinitionForSave>
    {
    }

    public class LineDefinitionEntry : LineDefinitionEntryForSave<LineDefinitionEntryContractDefinition, LineDefinitionEntryNotedContractDefinition, LineDefinitionEntryResourceDefinition>
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
