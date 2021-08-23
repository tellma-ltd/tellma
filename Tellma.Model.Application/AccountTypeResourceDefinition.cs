using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "AccountTypeResourceDefinition", GroupName = "AccountTypeResourceDefinitions")]
    public class AccountTypeResourceDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Account_ResourceDefinition")]
        [Required, ValidateRequired]
        public int? ResourceDefinitionId { get; set; }
    }

    public class AccountTypeResourceDefinition : AccountTypeResourceDefinitionForSave
    {
        [Required]
        public int? AccountTypeId { get; set; }

        [Display(Name = "Account_ResourceDefinition")]
        [ForeignKey(nameof(ResourceDefinitionId))]
        public ResourceDefinition ResourceDefinition { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
