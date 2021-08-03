using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "LineDefinitionEntry", GroupName = "LineDefinitionEntries")]
    public class LineDefinitionEntryForSave<TRelationDef, TResourceDef, TNotedRelationDef> : EntityWithKey<int>
    {
        [Display(Name = "LineDefinitionEntry_Direction")]
        [ChoiceList(new object[] { 
                (short)1, 
                (short)-1 }, 
            new string[] { 
                "Entry_Direction_Debit", 
                "Entry_Direction_Credit" })]
        [Required, ValidateRequired]
        public short? Direction { get; set; }

        [Display(Name = "LineDefinitionEntry_ParentAccountType")]
        [Required, ValidateRequired]
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
        [Required]
        public int? Index { get; set; }

        [Display(Name = "LineDefinitionEntry_LineDefinition")]
        [Required]
        public int? LineDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
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
