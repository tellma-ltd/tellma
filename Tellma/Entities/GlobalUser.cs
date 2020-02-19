using System.ComponentModel.DataAnnotations;

namespace Tellma.Entities
{
    public class DirectoryUser : EntityWithKey<int>
    {
        public string ExternalId { get; set; }

        [Display(Name = "User_Email")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [EmailAddress(ErrorMessage = nameof(EmailAddressAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string Email { get; set; }
    }
}
