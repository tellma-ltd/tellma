using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "LineDefinitionEntryRelationDefinition", Plural = "LineDefinitionEntryRelationDefinitions")]
    public class LineDefinitionEntryRelationDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "LineDefinitionEntryRelationDefinition_RelationDefinition")]
        [Required]
        [NotNull]
        public int? RelationDefinitionId { get; set; }
    }

    public class LineDefinitionEntryRelationDefinition : LineDefinitionEntryRelationDefinitionForSave
    {
        [Display(Name = "Entity_LineDefinitionEntry")]
        [NotNull]
        public int? LineDefinitionEntryId { get; set; }

        [Display(Name = "LineDefinitionEntryRelationDefinition_RelationDefinition")]
        [ForeignKey(nameof(RelationDefinitionId))]
        [NotNull]
        public RelationDefinition RelationDefinition { get; set; }

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
