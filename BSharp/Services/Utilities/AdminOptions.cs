using System.ComponentModel.DataAnnotations;

namespace BSharp.Services.Utilities
{
    public class AdminOptions
    {
        [EmailAddress]
        public string Email { get; set; }

        public string FullName { get; set; }

        public string Password { get; set; }
    }
}
