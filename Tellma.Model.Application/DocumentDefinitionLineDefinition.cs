using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "DocumentDefinitionLineDefinition", GroupName = "DocumentDefinitionLineDefinitions")]
    public class DocumentDefinitionLineDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "DocumentDefinitionLineDefinition_LineDefinition")]
        [Required, ValidateRequired]
        public int? LineDefinitionId { get; set; }

        [Display(Name = "DocumentDefinitionLineDefinition_IsVisibleByDefault")]
        public bool? IsVisibleByDefault { get; set; }
    }

    public class DocumentDefinitionLineDefinition : DocumentDefinitionLineDefinitionForSave
    {
        [Required]
        public int? Index { get; set; }

        [Display(Name = "DocumentDefinition")]
        [Required]
        public int? DocumentDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        [Display(Name = "DocumentDefinitionLineDefinition_LineDefinition")]
        [ForeignKey(nameof(LineDefinitionId))]
        public LineDefinition LineDefinition { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
