using System.ComponentModel.DataAnnotations;

namespace Tellma.Services.Email
{
    public class SendGridOptions
    {
        public string DefaultFromEmail { get; set; } = "dotnotreply@tellma.com";

        [Required]
        public string ApiKey { get; set; }
    }
}