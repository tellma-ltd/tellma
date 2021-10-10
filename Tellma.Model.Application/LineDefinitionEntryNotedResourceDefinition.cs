using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "LineDefinitionEntryNotedResourceDefinition", GroupName = "LineDefinitionEntryNotedResourceDefinitions")]
    public class LineDefinitionEntryNotedResourceDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "LineDefinitionEntryNotedResourceDefinition_NotedResourceDefinition")]
        [Required, ValidateRequired]
        public int? NotedResourceDefinitionId { get; set; }
    }

    public class LineDefinitionEntryNotedResourceDefinition : LineDefinitionEntryNotedResourceDefinitionForSave
    {
        [Display(Name = "Entity_LineDefinitionEntry")]
        [Required]
        public int? LineDefinitionEntryId { get; set; }

        [Display(Name = "LineDefinitionEntryNotedResourceDefinition_NotedResourceDefinition")]
        [ForeignKey(nameof(NotedResourceDefinitionId))]
        [Required]
        public ResourceDefinition NotedResourceDefinition { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        // For query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
