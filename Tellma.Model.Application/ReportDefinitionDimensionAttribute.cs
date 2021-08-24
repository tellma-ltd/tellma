using System.ComponentModel.DataAnnotations;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    public class ReportDefinitionDimensionAttributeForSave : EntityWithKey<int>
    {
        [Display(Name = "ReportDefinition_Expression")]
        [Required]
        [StringLength(1024)]
        public string Expression { get; set; }

        [Display(Name = "ReportDefinition_Localize")]
        [Required]
        public bool? Localize { get; set; }

        [Display(Name = "Label")]
        [StringLength(255)]
        public string Label { get; set; }

        [Display(Name = "Label")]
        [StringLength(255)]
        public string Label2 { get; set; }

        [Display(Name = "Label")]
        [StringLength(255)]
        public string Label3 { get; set; }

        [Display(Name = "ReportDefinition_OrderDirection")]
        [ChoiceList(new object[] { 
                "asc", 
                "desc" },
            new string[] { 
                "ReportDefinition_OrderDirection_Asc", 
                "ReportDefinition_OrderDirection_Desc" 
            })]
        public string OrderDirection { get; set; }
    }
    public class ReportDefinitionDimensionAttribute : ReportDefinitionDimensionAttributeForSave
    {
        [Required]
        public int? ReportDefinitionDimensionId { get; set; }

        [Required]
        public int? Index { get; set; }
    }
}
