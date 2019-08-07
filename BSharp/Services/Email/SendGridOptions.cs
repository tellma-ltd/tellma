using System.ComponentModel.DataAnnotations;

namespace BSharp.Services.Email
{
    public class SendGridOptions
    {
        public string DefaultFromEmail { get; set; } = "dotnotreply@bsharp.online";

        [Required]
        public string ApiKey { get; set; }
    }
}