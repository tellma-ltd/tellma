using BSharp.Controllers.Misc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Controllers.DTO
{
    [CollectionName("Views")]
    public class ViewForSave : DtoForSaveKeyBase<string>
    {
        [Display(Name = "Permissions")]
        public List<Permission> Permissions { get; set; }
    }

    public class View : ViewForSave
    {
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Name2")]
        public string Name2 { get; set; }

        [Display(Name = "Code")]
        public string Code { get; set; }

        [Display(Name = "IsActive")]
        public bool? IsActive { get; set; }

        // Never displayed
        public List<string> AllowedPermissionLevels { get; set; }
    }
}
