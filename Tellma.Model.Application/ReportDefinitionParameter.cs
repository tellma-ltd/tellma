using System.ComponentModel.DataAnnotations;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    public class ReportDefinitionParameterForSave : EntityWithKey<int>
    {
        [Display(Name = "Parameter_Key")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string Key { get; set; }

        [Display(Name = "Label")]
        [StringLength(255)]
        public string Label { get; set; }

        [Display(Name = "Label")]
        [StringLength(255)]
        public string Label2 { get; set; }

        [Display(Name = "Label")]
        [StringLength(255)]
        public string Label3 { get; set; }

        [Display(Name = "Parameter_Visibility")]
        [VisibilityChoiceList]
        public string Visibility { get; set; }

        [Display(Name = "ReportDefinition_DefaultExpression")]
        [StringLength(1024)]
        public string DefaultExpression { get; set; }

        [Display(Name = "Definition_Control")]
        [StringLength(50)]
        public string Control { get; set; }

        [Display(Name = "Definition_ControlOptions")]
        [StringLength(1024)]
        public string ControlOptions { get; set; }
    }

    public class ReportDefinitionParameter : ReportDefinitionParameterForSave
    {
        [Required]
        public int? ReportDefinitionId { get; set; }

        [Required]
        public int? Index { get; set; }
    }
}
