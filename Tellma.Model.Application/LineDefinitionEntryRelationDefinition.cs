using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "LineDefinitionEntryRelationDefinition", GroupName = "LineDefinitionEntryRelationDefinitions")]
    public class LineDefinitionEntryRelationDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "LineDefinitionEntryRelationDefinition_RelationDefinition")]
        [Required, ValidateRequired]
        public int? RelationDefinitionId { get; set; }
    }

    public class LineDefinitionEntryRelationDefinition : LineDefinitionEntryRelationDefinitionForSave
    {
        [Display(Name = "Entity_LineDefinitionEntry")]
        [Required]
        public int? LineDefinitionEntryId { get; set; }

        [Display(Name = "LineDefinitionEntryRelationDefinition_RelationDefinition")]
        [ForeignKey(nameof(RelationDefinitionId))]
        [Required]
        public RelationDefinition RelationDefinition { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        // For query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
