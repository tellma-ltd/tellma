using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "LineDefinitionEntryNotedAgentDefinition", GroupName = "LineDefinitionEntryNotedAgentDefinitions")]
    public class LineDefinitionEntryNotedAgentDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "LineDefinitionEntryNotedAgentDefinition_NotedAgentDefinition")]
        [Required, ValidateRequired]
        public int? NotedAgentDefinitionId { get; set; }
    }

    public class LineDefinitionEntryNotedAgentDefinition : LineDefinitionEntryNotedAgentDefinitionForSave
    {
        [Display(Name = "Entity_LineDefinitionEntry")]
        [Required]
        public int? LineDefinitionEntryId { get; set; }

        [Display(Name = "LineDefinitionEntryNotedAgentDefinition_NotedAgentDefinition")]
        [ForeignKey(nameof(NotedAgentDefinitionId))]
        [Required]
        public AgentDefinition NotedAgentDefinition { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        // For query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
