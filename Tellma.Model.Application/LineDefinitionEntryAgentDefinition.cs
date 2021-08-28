using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "LineDefinitionEntryAgentDefinition", GroupName = "LineDefinitionEntryAgentDefinitions")]
    public class LineDefinitionEntryAgentDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "LineDefinitionEntryAgentDefinition_AgentDefinition")]
        [Required, ValidateRequired]
        public int? AgentDefinitionId { get; set; }
    }

    public class LineDefinitionEntryAgentDefinition : LineDefinitionEntryAgentDefinitionForSave
    {
        [Display(Name = "Entity_LineDefinitionEntry")]
        [Required]
        public int? LineDefinitionEntryId { get; set; }

        [Display(Name = "LineDefinitionEntryAgentDefinition_AgentDefinition")]
        [ForeignKey(nameof(AgentDefinitionId))]
        [Required]
        public AgentDefinition AgentDefinition { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        // For query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
