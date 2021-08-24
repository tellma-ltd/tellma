using System.ComponentModel.DataAnnotations;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    public class ReportDefinitionMeasureForSave : EntityWithKey<int>
    {
        [Display(Name = "ReportDefinition_Expression")]
        [Required, ValidateRequired]
        [StringLength(1024)]
        public string Expression { get; set; }

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
        [ChoiceList(new object[] { "asc", "desc" },
            new string[] { "ReportDefinition_OrderDirection_asc", "ReportDefinition_OrderDirection_desc" })]
        public string OrderDirection { get; set; }

        [Display(Name = "Definition_Control")]
        [StringLength(50)]
        public string Control { get; set; }

        [Display(Name = "Definition_ControlOptions")]
        [StringLength(1024)]
        public string ControlOptions { get; set; }

        [Display(Name = "ReportDefinition_DangerWhen")]
        [StringLength(1024)]
        public string DangerWhen { get; set; }

        [Display(Name = "ReportDefinition_WarningWhen")]
        [StringLength(1024)]
        public string WarningWhen { get; set; }

        [Display(Name = "ReportDefinition_SuccessWhen")]
        [StringLength(1024)]
        public string SuccessWhen { get; set; }
    }

    public class ReportDefinitionMeasure : ReportDefinitionMeasureForSave
    {
        [Required]
        public int? ReportDefinitionId { get; set; }

        [Required]
        public int? Index { get; set; }
    }
}
