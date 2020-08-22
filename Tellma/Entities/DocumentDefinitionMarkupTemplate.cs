using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "DocumentDefinitionMarkupTemplate", Plural = "DocumentDefinitionMarkupTemplates")]
    public class DocumentDefinitionMarkupTemplateForSave : EntityWithKey<int>
    {
        [Display(Name = "DocumentDefinitionMarkupTemplate_MarkupTemplate")]
        [Required]
        public int? MarkupTemplateId { get; set; }
    }

    public class DocumentDefinitionMarkupTemplate : DocumentDefinitionMarkupTemplateForSave
    {
        [AlwaysAccessible]
        public int? Index { get; set; }

        [Display(Name = "DocumentDefinition")]
        public int? DocumentDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        [Display(Name = "DocumentDefinitionMarkupTemplate_MarkupTemplate")]
        [ForeignKey(nameof(MarkupTemplateId))]
        public MarkupTemplate MarkupTemplate { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
