using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "AccountTypeRelationDefinition", Plural = "AccountTypeRelationDefinitions")]
    public class AccountTypeRelationDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Account_RelationDefinition")]
        [NotNull]
        [Required]
        public int? RelationDefinitionId { get; set; }
    }

    public class AccountTypeRelationDefinition : AccountTypeRelationDefinitionForSave
    {
        [NotNull]
        public int? AccountTypeId { get; set; }

        [Display(Name = "Account_RelationDefinition")]
        [ForeignKey(nameof(RelationDefinitionId))]
        public RelationDefinition RelationDefinition { get; set; }

        [Display(Name = "ModifiedBy")]
        [NotNull]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
