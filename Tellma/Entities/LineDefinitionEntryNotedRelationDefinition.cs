using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "LineDefinitionEntryNotedRelationDefinition", Plural = "LineDefinitionEntryNotedRelationDefinitions")]
    public class LineDefinitionEntryNotedRelationDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "LineDefinitionEntryNotedRelationDefinition_NotedRelationDefinition")]
        [Required]
        [NotNull]
        public int? NotedRelationDefinitionId { get; set; }
    }

    public class LineDefinitionEntryNotedRelationDefinition : LineDefinitionEntryNotedRelationDefinitionForSave
    {
        [Display(Name = "Entity_LineDefinitionEntry")]
        [NotNull]
        public int? LineDefinitionEntryId { get; set; }

        [Display(Name = "LineDefinitionEntryNotedRelationDefinition_NotedRelationDefinition")]
        [ForeignKey(nameof(NotedRelationDefinitionId))]
        [NotNull]
        public RelationDefinition NotedRelationDefinition { get; set; }

        [Display(Name = "CreatedAt")]
        [NotNull]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [NotNull]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [NotNull]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [NotNull]
        public int? ModifiedById { get; set; }

        // For query

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
