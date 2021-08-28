using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "AccountTypeNotedAgentDefinition", GroupName = "AccountTypeNotedAgentDefinitions")]
    public class AccountTypeNotedAgentDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Account_NotedAgentDefinition")]
        [Required, ValidateRequired]
        public int? NotedAgentDefinitionId { get; set; }
    }

    public class AccountTypeNotedAgentDefinition : AccountTypeNotedAgentDefinitionForSave
    {
        [Required]
        public int? AccountTypeId { get; set; }

        [Display(Name = "Account_NotedAgentDefinition")]
        [ForeignKey(nameof(NotedAgentDefinitionId))]
        public AgentDefinition NotedAgentDefinition { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
