using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "AccountTypeRelationDefinition", GroupName = "AccountTypeRelationDefinitions")]
    public class AccountTypeRelationDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Account_RelationDefinition")]
        [Required, ValidateRequired]
        public int? RelationDefinitionId { get; set; }
    }

    public class AccountTypeRelationDefinition : AccountTypeRelationDefinitionForSave
    {
        [Required]
        public int? AccountTypeId { get; set; }

        [Display(Name = "Account_RelationDefinition")]
        [ForeignKey(nameof(RelationDefinitionId))]
        public RelationDefinition RelationDefinition { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
