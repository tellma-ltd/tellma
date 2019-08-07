using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.EntityModel
{
    [StrongEntity]
    public class ViewForSave<TPermission> : EntityWithKey<string>
    {
        [ForeignKey(nameof(Permission.ViewId))]
        [Display(Name = "Permissions")]
        public List<TPermission> Permissions { get; set; }
    }

    public class ViewForSave : ViewForSave<PermissionForSave>
    {
    }

    public class View : ViewForSave<Permission>
    {
        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [AlwaysAccessible]
        public string Name { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        [AlwaysAccessible]
        public string Name2 { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        [AlwaysAccessible]
        public string Name3 { get; set; }

        [Display(Name = "Code")]
        [AlwaysAccessible]
        public string Code { get; set; }

        [Display(Name = "IsActive")]
        [AlwaysAccessible]
        public bool? IsActive { get; set; }

        // For Query

        [Display(Name = "Permissions")]
        [ForeignKey(nameof(Permission.ViewId))]
        public List<ViewAction> Actions { get; set; }
    }

}
