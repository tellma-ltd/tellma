using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "LookupDefinitionReportDefinition", Plural = "LookupDefinitionReportDefinitions")]
    public class LookupDefinitionReportDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Definition_ReportDefinition")]
        [Required]
        public int? ReportDefinitionId { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
    }

    public class LookupDefinitionReportDefinition : LookupDefinitionReportDefinitionForSave
    {
        [AlwaysAccessible]
        public int? Index { get; set; }

        public int? LookupDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        [Display(Name = "Definition_ReportDefinition")]
        [ForeignKey(nameof(ReportDefinitionId))]
        public ReportDefinition ReportDefinition { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
