using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "AccountTypeNotedRelationDefinition", Plural = "AccountTypeNotedRelationDefinitions")]
    public class AccountTypeNotedRelationDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Account_NotedRelationDefinition")]
        [NotNull]
        [Required]
        public int? NotedRelationDefinitionId { get; set; }
    }

    public class AccountTypeNotedRelationDefinition : AccountTypeNotedRelationDefinitionForSave
    {
        [NotNull]
        public int? AccountTypeId { get; set; }

        [Display(Name = "Account_NotedRelationDefinition")]
        [ForeignKey(nameof(NotedRelationDefinitionId))]
        public RelationDefinition NotedRelationDefinition { get; set; }

        [Display(Name = "ModifiedBy")]
        [NotNull]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
