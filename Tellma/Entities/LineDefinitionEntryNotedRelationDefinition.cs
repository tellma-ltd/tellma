using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    public class LineDefinitionEntryNotedRelationDefinitionForSave : EntityWithKey<int>
    {
        public int? NotedRelationDefinitionId { get; set; }
    }

    public class LineDefinitionEntryNotedRelationDefinition : LineDefinitionEntryNotedRelationDefinitionForSave
    {
        public int? LineDefinitionEntryId { get; set; }

        [ForeignKey(nameof(NotedRelationDefinitionId))]
        public RelationDefinition NotedRelationDefinition { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
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
