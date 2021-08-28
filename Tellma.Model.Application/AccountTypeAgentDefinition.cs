using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "AccountTypeAgentDefinition", GroupName = "AccountTypeAgentDefinitions")]
    public class AccountTypeAgentDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Account_AgentDefinition")]
        [Required, ValidateRequired]
        public int? AgentDefinitionId { get; set; }
    }

    public class AccountTypeAgentDefinition : AccountTypeAgentDefinitionForSave
    {
        [Required]
        public int? AccountTypeId { get; set; }

        [Display(Name = "Account_AgentDefinition")]
        [ForeignKey(nameof(AgentDefinitionId))]
        public AgentDefinition AgentDefinition { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
