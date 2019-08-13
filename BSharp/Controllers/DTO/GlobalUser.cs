using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Controllers.Dto
{
    public class GlobalUserForSave<TTenantMembership> : DtoForSaveKeyBase<int?>
    {
        [EmailAddress(ErrorMessage = nameof(EmailAddressAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "User_Email")]
        public string Email { get; set; }

        [Display(Name = "User_Companies")]
        public List<TTenantMembership> Memberships { get; set; }
    }

    public class GlobalUserForSave : GlobalUserForSave<TenantMembershipForSave> { }

    public class GlobalUser : GlobalUserForSave<TenantMembership>
    {
        public string ExternalId { get; set; }
    }
}
