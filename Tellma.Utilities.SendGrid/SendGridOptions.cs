using System.ComponentModel.DataAnnotations;

namespace Tellma.Utilities.SendGrid
{
    public class SendGridOptions
    {
        public string DefaultFromEmail { get; set; } = "dotnotreply@tellma.com";

        public string DefaultFromName { get; set; } = "Tellma ERP";

        [Required]
        public string ApiKey { get; set; }

        public bool CallbacksEnabled { get; set; }

        public string VerificationKey { get; set; }
    }
}