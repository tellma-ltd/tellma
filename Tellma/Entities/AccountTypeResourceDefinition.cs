using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "AccountTypeResourceDefinition", Plural = "AccountTypeResourceDefinitions")]
    public class AccountTypeResourceDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Account_ResourceDefinition")]
        [Required]
        public int? ResourceDefinitionId { get; set; }
    }

    public class AccountTypeResourceDefinition : AccountTypeResourceDefinitionForSave
    {
        public int? AccountTypeId { get; set; }

        [Display(Name = "Account_ResourceDefinition")]
        [ForeignKey(nameof(ResourceDefinitionId))]
        public ResourceDefinition ResourceDefinition { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
