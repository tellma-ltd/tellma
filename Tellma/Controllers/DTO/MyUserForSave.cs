using Tellma.Entities;
using System.ComponentModel.DataAnnotations;

namespace Tellma.Controllers.Dto
{
    /// <summary>
    /// Carries the preferences that the user can modify about themselves within a company
    /// </summary>
    public class MyUserForSave : Entity
    {
        [Display(Name = "Image")]
        public byte[] Image { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [Required(ErrorMessage = Services.Utilities.Constants.Error_Field0IsRequired)]
        [StringLength(255, ErrorMessage = Services.Utilities.Constants.Error_Field0LengthMaximumOf1)]
        public string Name { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        [StringLength(255, ErrorMessage = Services.Utilities.Constants.Error_Field0LengthMaximumOf1)]
        public string Name2 { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        [StringLength(255, ErrorMessage = Services.Utilities.Constants.Error_Field0LengthMaximumOf1)]
        public string Name3 { get; set; }

        [Display(Name = "User_PreferredLanguage")]
        [StringLength(2, ErrorMessage = Services.Utilities.Constants.Error_Field0LengthMaximumOf1)]
        [CultureChoiceList]
        public string PreferredLanguage { get; set; }
    }
}
