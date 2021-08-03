using System.ComponentModel.DataAnnotations;

namespace Tellma.Api.Dto
{
    /// <summary>
    /// Carries the preferences that the admin user can modify about themselves.
    /// </summary>
    public class MyAdminUserForSave
    {
        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}
