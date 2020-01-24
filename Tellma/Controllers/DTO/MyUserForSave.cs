using Tellma.Entities;
using System.ComponentModel.DataAnnotations;

namespace Tellma.Controllers.Dto
{
    /// <summary>
    /// Carries the preferences that the user can modify about themselves within a company
    /// </summary>
    public class MyUserForSave
    {
        [Display(Name = "Image")]
        public byte[] Image { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string Name { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string Name2 { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string Name3 { get; set; }

        [Display(Name = "User_PreferredLanguage")]
        [StringLength(2, ErrorMessage = nameof(StringLengthAttribute))]
        [Culture]
        public string PreferredLanguage { get; set; }
    }
}
