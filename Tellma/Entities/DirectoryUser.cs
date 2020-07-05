using System.ComponentModel.DataAnnotations;

namespace Tellma.Entities
{
    public class DirectoryUser : EntityWithKey<int>
    {
        public string ExternalId { get; set; }

        [Display(Name = "User_Email")]
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }
    }
}
