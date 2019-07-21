using BSharp.Controllers.Misc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Controllers.DTO
{
    [StrongDto]
    public class ViewForSave<TPermission, TRequiredSignature> : DtoForSaveKeyBase<string>
    {
        [Display(Name = "Signatures")]
        public List<TRequiredSignature> Signatures { get; set; } = new List<TRequiredSignature>();

        [Display(Name = "Permissions")]
        public List<TPermission> Permissions { get; set; } = new List<TPermission>();
    }

    public class ViewForSave : ViewForSave<PermissionForSave, RequiredSignatureForSave>
    {
    }

    public class View<TPermission, TRequiredSignature> : ViewForSave<TPermission, TRequiredSignature>
    {
        [BasicField]
        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        public string Name { get; set; }

        [BasicField]
        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        public string Name2 { get; set; }

        //[BasicField]
        //[MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        //public string Name3 { get; set; }

        [BasicField]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [BasicField]
        [Display(Name = "IsActive")]
        public bool? IsActive { get; set; }

        // Never displayed
        [BasicField]
        public string AllowedPermissionLevels { get; set; }

        [BasicField]
        public bool? SupportsCriteria { get; set; }

        [BasicField]
        public bool? SupportsMask { get; set; }
    }

    public class View : View<Permission, RequiredSignature>
    {
    }
}
