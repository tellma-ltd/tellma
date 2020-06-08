using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "LineDefinitionEntry", Plural = "LineDefinitionEntries")]
    public class LineDefinitionEntryForSave<TContractDef, TNotedContractDef, TResourceDef, TAccountType> : EntityWithKey<int>
    {
        public short? Direction { get; set; }

        public int? EntryTypeId { get; set; }

        [ForeignKey(nameof(LineDefinitionEntryContractDefinition.LineDefinitionEntryId))]
        public List<TContractDef> ContractDefinitions { get; set; }

        [ForeignKey(nameof(LineDefinitionEntryNotedContractDefinition.LineDefinitionEntryId))]
        public List<TNotedContractDef> NotedContractDefinitions { get; set; }

        [ForeignKey(nameof(LineDefinitionEntryResourceDefinition.LineDefinitionEntryId))]
        public List<TResourceDef> ResourceDefinitions { get; set; }

        [ForeignKey(nameof(LineDefinitionEntryAccountType.LineDefinitionEntryId))]
        public List<TAccountType> AccountTypes { get; set; }
    }

    public class LineDefinitionEntryForSave : LineDefinitionEntryForSave<LineDefinitionEntryContractDefinitionForSave, LineDefinitionEntryNotedContractDefinitionForSave, LineDefinitionEntryResourceDefinitionForSave, LineDefinitionEntryAccountTypeForSave>
    {
    }

    public class LineDefinitionEntry : LineDefinitionEntryForSave<LineDefinitionEntryContractDefinition, LineDefinitionEntryNotedContractDefinition, LineDefinitionEntryResourceDefinition, LineDefinitionEntryAccountType>
    {
        [AlwaysAccessible]
        public int? Index { get; set; }

        public int? LineDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
