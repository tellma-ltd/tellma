using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "AccountTypeNotedResourceDefinition", GroupName = "AccountTypeNotedResourceDefinitions")]
    public class AccountTypeNotedResourceDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Account_NotedResourceDefinition")]
        [Required, ValidateRequired]
        public int? NotedResourceDefinitionId { get; set; }
    }

    public class AccountTypeNotedResourceDefinition : AccountTypeNotedResourceDefinitionForSave
    {
        [Required]
        public int? AccountTypeId { get; set; }

        [Display(Name = "Account_NotedResourceDefinition")]
        [ForeignKey(nameof(NotedResourceDefinitionId))]
        public ResourceDefinition NotedResourceDefinition { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
