using System.ComponentModel.DataAnnotations;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "IfrsConcept", GroupName = "IfrsConcepts")]
    public class IfrsConcept : EntityWithKey<int>
    {
        [Display(Name = "Code")]
        [StringLength(255)]
        public string Code { get; set; }

        [Display(Name = "Label")]
        [Required]
        [StringLength(1024)]
        public string Label { get; set; }

        [Display(Name = "Label")]
        [StringLength(1024)]
        public string Label2 { get; set; }

        [Display(Name = "Label")]
        [StringLength(1024)]
        public string Label3 { get; set; }

        [Display(Name = "IfrsConcept_Documentation")]
        public string Documentation { get; set; }

        [Display(Name = "IfrsConcept_Documentation")]
        public string Documentation2 { get; set; }

        [Display(Name = "IfrsConcept_Documentation")]
        public string Documentation3 { get; set; }
    }
}
