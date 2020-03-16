using Tellma.Entities;
using System.ComponentModel.DataAnnotations;

namespace Tellma.Controllers.Dto
{
    /// <summary>
    /// Carries the preferences that the user can modify about themselves within a company
    /// </summary>
    public class MyAdminUserForSave
    {
        [Display(Name = "Name")]
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string Name { get; set; }
    }
}
