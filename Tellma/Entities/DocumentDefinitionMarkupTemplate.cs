using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "DocumentDefinitionMarkupTemplate", Plural = "DocumentDefinitionMarkupTemplates")]
    public class DocumentDefinitionMarkupTemplateForSave : EntityWithKey<int>
    {
        public int? MarkupTemplateId { get; set; }
    }

    public class DocumentDefinitionMarkupTemplate : DocumentDefinitionMarkupTemplateForSave
    {
        public int? DocumentDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        [ForeignKey(nameof(MarkupTemplateId))]
        public MarkupTemplate MarkupTemplate { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
