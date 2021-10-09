using System.ComponentModel.DataAnnotations;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    public class PrintingTemplateParameterForSave : EntityWithKey<int>
    {
        [Display(Name = "Parameter_Key")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string Key { get; set; }

        [Display(Name = "Label")]
        [StringLength(255)]
        [Required, ValidateRequired]
        public string Label { get; set; }

        [Display(Name = "Label")]
        [StringLength(255)]
        public string Label2 { get; set; }

        [Display(Name = "Label")]
        [StringLength(255)]
        public string Label3 { get; set; }

        [Display(Name = "Parameter_Visibility")]
        [Required]
        public bool? IsRequired { get; set; }

        [Display(Name = "Definition_Control")]
        [StringLength(50)]
        [Required, ValidateRequired]
        public string Control { get; set; }

        [Display(Name = "Definition_ControlOptions")]
        [StringLength(1024)]
        public string ControlOptions { get; set; }
    }

    public class PrintingTemplateParameter : PrintingTemplateParameterForSave
    {
        [Required]
        public int? PrintingTemplateId { get; set; }

        [Required]
        public int? Index { get; set; }
    }
}
