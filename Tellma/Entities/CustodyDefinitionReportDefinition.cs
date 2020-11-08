using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "CustodyDefinitionReportDefinition", Plural = "CustodyDefinitionReportDefinitions")]
    public class CustodyDefinitionReportDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Definition_ReportDefinition")]
        [Required]
        public int? ReportDefinitionId { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
    }

    public class CustodyDefinitionReportDefinition : CustodyDefinitionReportDefinitionForSave
    {
        [AlwaysAccessible]
        public int? Index { get; set; }

        public int? CustodyDefinitionId { get; set; }

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
