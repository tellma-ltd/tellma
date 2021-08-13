using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "LineDefinitionEntryNotedRelationDefinition", GroupName = "LineDefinitionEntryNotedRelationDefinitions")]
    public class LineDefinitionEntryNotedRelationDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "LineDefinitionEntryNotedRelationDefinition_NotedRelationDefinition")]
        [Required, ValidateRequired]
        public int? NotedRelationDefinitionId { get; set; }
    }

    public class LineDefinitionEntryNotedRelationDefinition : LineDefinitionEntryNotedRelationDefinitionForSave
    {
        [Display(Name = "Entity_LineDefinitionEntry")]
        [Required]
        public int? LineDefinitionEntryId { get; set; }

        [Display(Name = "LineDefinitionEntryNotedRelationDefinition_NotedRelationDefinition")]
        [ForeignKey(nameof(NotedRelationDefinitionId))]
        [Required]
        public RelationDefinition NotedRelationDefinition { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        // For query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
