using System.ComponentModel.DataAnnotations;

namespace Tellma.Api.Dto
{
    /// <summary>
    /// Carries the preferences that the user can modify about themselves within a company.
    /// </summary>
    public class MyUserForSave
    {
        [Display(Name = "Image")]
        public byte[] Image { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Name")]
        public string Name2 { get; set; }

        [Display(Name = "Name")]
        public string Name3 { get; set; }

        [Display(Name = "User_PreferredLanguage")]
        public string PreferredLanguage { get; set; }
    }
}
