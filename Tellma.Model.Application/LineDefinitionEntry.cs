using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "LineDefinitionEntry", GroupName = "LineDefinitionEntries")]
    public class LineDefinitionEntryForSave<TAgentDef, TResourceDef, TNotedAgentDef> : EntityWithKey<int>
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

        [Display(Name = "LineDefinitionEntry_AgentDefinitions")]
        [ForeignKey(nameof(LineDefinitionEntryAgentDefinition.LineDefinitionEntryId))]
        public List<TAgentDef> AgentDefinitions { get; set; }

        [Display(Name = "LineDefinitionEntry_ResourceDefinitions")]
        [ForeignKey(nameof(LineDefinitionEntryResourceDefinition.LineDefinitionEntryId))]
        public List<TResourceDef> ResourceDefinitions { get; set; }

        [Display(Name = "LineDefinitionEntry_NotedAgentDefinitions")]
        [ForeignKey(nameof(LineDefinitionEntryNotedAgentDefinition.LineDefinitionEntryId))]
        public List<TNotedAgentDef> NotedAgentDefinitions { get; set; }
    }

    public class LineDefinitionEntryForSave : LineDefinitionEntryForSave<LineDefinitionEntryAgentDefinitionForSave, LineDefinitionEntryResourceDefinitionForSave, LineDefinitionEntryNotedAgentDefinitionForSave>
    {
    }

    public class LineDefinitionEntry : LineDefinitionEntryForSave<LineDefinitionEntryAgentDefinition, LineDefinitionEntryResourceDefinition, LineDefinitionEntryNotedAgentDefinition>
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
