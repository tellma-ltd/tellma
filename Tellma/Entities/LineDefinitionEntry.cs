using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "LineDefinitionEntry", Plural = "LineDefinitionEntries")]
    public class LineDefinitionEntryForSave<TRelationDef, TResourceDef, TNotedRelationDef> : EntityWithKey<int>
    {
        [Display(Name = "LineDefinitionEntry_Direction")]
        [ChoiceList(new object[] { (short)1, (short)-1 }, new string[] { "Entry_Direction_Debit", "Entry_Direction_Credit" })]
        [Required]
        [NotNull]
        public short? Direction { get; set; }

        [Display(Name = "LineDefinitionEntry_ParentAccountType")]
        [Required]
        [NotNull]
        public int? ParentAccountTypeId { get; set; }

        [Display(Name = "LineDefinitionEntry_EntryType")]
        public int? EntryTypeId { get; set; }

        [Display(Name = "LineDefinitionEntry_RelationDefinitions")]
        [ForeignKey(nameof(LineDefinitionEntryRelationDefinition.LineDefinitionEntryId))]
        public List<TRelationDef> RelationDefinitions { get; set; }

        [Display(Name = "LineDefinitionEntry_ResourceDefinitions")]
        [ForeignKey(nameof(LineDefinitionEntryResourceDefinition.LineDefinitionEntryId))]
        public List<TResourceDef> ResourceDefinitions { get; set; }

        [Display(Name = "LineDefinitionEntry_NotedRelationDefinitions")]
        [ForeignKey(nameof(LineDefinitionEntryNotedRelationDefinition.LineDefinitionEntryId))]
        public List<TNotedRelationDef> NotedRelationDefinitions { get; set; }
    }

    public class LineDefinitionEntryForSave : LineDefinitionEntryForSave<LineDefinitionEntryRelationDefinitionForSave, LineDefinitionEntryResourceDefinitionForSave, LineDefinitionEntryNotedRelationDefinitionForSave>
    {
    }

    public class LineDefinitionEntry : LineDefinitionEntryForSave<LineDefinitionEntryRelationDefinition, LineDefinitionEntryResourceDefinition, LineDefinitionEntryNotedRelationDefinition>
    {
        [AlwaysAccessible]
        [NotNull]
        public int? Index { get; set; }

        [Display(Name = "LineDefinitionEntry_LineDefinition")]
        [NotNull]
        public int? LineDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        [NotNull]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "LineDefinitionEntry_ParentAccountType")]
        [ForeignKey(nameof(ParentAccountTypeId))]
        public AccountType ParentAccountType { get; set; }

        [Display(Name = "LineDefinitionEntry_EntryType")]
        [ForeignKey(nameof(EntryTypeId))]
        public EntryType EntryType { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
