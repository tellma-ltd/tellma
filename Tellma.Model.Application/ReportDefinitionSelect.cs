using System.ComponentModel.DataAnnotations;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    public class ReportDefinitionSelectForSave : EntityWithKey<int>
    {
        [Display(Name = "ReportDefinition_Expression")]
        [Required, ValidateRequired]
        [StringLength(255)]
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

        [Display(Name = "Definition_Control")]
        [StringLength(50)]
        public string Control { get; set; }

        [Display(Name = "Definition_ControlOptions")]
        [StringLength(1024)]
        public string ControlOptions { get; set; }
    }

    public class ReportDefinitionSelect : ReportDefinitionSelectForSave
    {
        [Required]
        public int? ReportDefinitionId { get; set; }

        [Required]
        public int? Index { get; set; }
    }
}
