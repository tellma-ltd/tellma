using Tellma.Entities;
using System.ComponentModel.DataAnnotations;

namespace Tellma.Controllers.Dto
{
    /// <summary>
    /// Carries the preferences that the user can modify about themselves within a company
    /// </summary>
    public class MyAdminUserForSave : Entity
    {
        [Display(Name = "Name")]
        [Required(ErrorMessage = Services.Utilities.Constants.Error_Field0IsRequired)]
        [StringLength(255, ErrorMessage = Services.Utilities.Constants.Error_Field0LengthMaximumOf1)]
        public string Name { get; set; }
    }
}
