using BSharp.Controllers.Misc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Controllers.DTO
{
    [CollectionName("Views")]
    public class ViewForSave<TPermission> : DtoForSaveKeyBase<string>
    {
        [Display(Name = "Permissions")]
        public List<Permission> Permissions { get; set; }
    }

    public class ViewForSave : ViewForSave<PermissionForSave>
    {

    }

    public class View : ViewForSave<Permission>
    {
        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        public string Name { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        public string Name2 { get; set; }

        [Display(Name = "Code")]
        public string Code { get; set; }

        [Display(Name = "IsActive")]
        public bool? IsActive { get; set; }

        // Never displayed
        public string AllowedPermissionLevels { get; set; }
    }
}
