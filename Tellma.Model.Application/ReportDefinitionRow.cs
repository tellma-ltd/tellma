using System.ComponentModel.DataAnnotations;

namespace Tellma.Model.Application
{
    public class ReportDefinitionRowForSave : ReportDefinitionDimension<ReportDefinitionDimensionAttributeForSave>
    {
    }

    public class ReportDefinitionRow : ReportDefinitionDimension<ReportDefinitionDimensionAttribute>
    {
        [Required]
        public int? ReportDefinitionId { get; set; }

        [Required]
        public int? Index { get; set; }
    }
}
