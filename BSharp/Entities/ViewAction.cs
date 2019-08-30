using BSharp.Services.Utilities;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Entities
{
    public class ViewAction : EntityWithKey<int>
    {
        [AlwaysAccessible]
        public string ViewId { get; set; }

        [Display(Name = "Permission_Action")]
        [ChoiceList(new object[] { Constants.Read, Constants.Update, "Delete", "IsActive", "ResendInvitationEmail" },
            new string[] { "Permission_Read", "Permission_Update", "Permission_Delete", "Permission_IsActive", "ResendInvitationEmail" })]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [AlwaysAccessible]
        public string Action { get; set; }

        [AlwaysAccessible]
        public bool? SupportsCriteria { get; set; }

        [AlwaysAccessible]
        public bool? SupportsMask { get; set; }
    }

}
