using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "AccountTypeNotedRelationDefinition", GroupName = "AccountTypeNotedRelationDefinitions")]
    public class AccountTypeNotedRelationDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Account_NotedRelationDefinition")]
        [Required]
        public int? NotedRelationDefinitionId { get; set; }
    }

    public class AccountTypeNotedRelationDefinition : AccountTypeNotedRelationDefinitionForSave
    {
        [Required]
        public int? AccountTypeId { get; set; }

        [Display(Name = "Account_NotedRelationDefinition")]
        [ForeignKey(nameof(NotedRelationDefinitionId))]
        public RelationDefinition NotedRelationDefinition { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
