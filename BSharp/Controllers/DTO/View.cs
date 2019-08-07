using BSharp.Controllers.Misc;
using BSharp.Services.Utilities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Controllers.DTO
{
    // TODO: Delete

    [StrongEntity]
    public class ViewForSave<TPermission, TRequiredSignature> : DtoForSaveKeyBase<string>
    {
        [NavigationProperty(ForeignKey = nameof(RequiredSignature.ViewId))]
        [Display(Name = "Signatures")]
        public List<TRequiredSignature> Signatures { get; set; }

        [NavigationProperty(ForeignKey = nameof(Permission.ViewId))]
        [Display(Name = "Permissions")]
        public List<TPermission> Permissions { get; set; }
    }

    public class ViewForSave : ViewForSave<PermissionForSave, RequiredSignatureForSave>
    {
    }

    public class View : ViewForSave<Permission, RequiredSignature>
    {
        [BasicField]
        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        public string Name { get; set; }

        [BasicField]
        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        public string Name2 { get; set; }

        [BasicField]
        [MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        public string Name3 { get; set; }

        [BasicField]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [BasicField]
        [Display(Name = "IsActive")]
        public bool? IsActive { get; set; }

        //// Never displayed
        //[BasicField]
        //public string AllowedPermissionLevels { get; set; }

        // For Query
        [NavigationProperty(ForeignKey = nameof(Permission.ViewId))]
        [Display(Name = "Permissions")]
        public List<ViewAction> Actions { get; set; }
    }

    public class ViewAction : DtoForSaveKeyBase<string>
    {
        [BasicField]
        public string ViewId { get; set; }

        [BasicField]
        [ChoiceList(new object[] { Constants.Read, Constants.Update, "IsActive", "ResendInvitationEmail" },
            new string[] { "Permission_Read", "Permission_Update", "Permission_IsActive", "ResendInvitationEmail" })]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [Display(Name = "Permission_Action")]
        public string Action { get; set; }

        [BasicField]
        public bool? SupportsCriteria { get; set; }

        [BasicField]
        public bool? SupportsMask { get; set; }
    }
}
