namespace Tellma.Services.Utilities
{
    public class GlobalOptions
    {
        public bool EmailEnabled { get; set; } = true;

        public bool EmbeddedIdentityServerEnabled { get; set; } = true;

        public bool EmbeddedClientApplicationEnabled { get; set; } = true;

        public LocalizationOptions Localization { get; set; }

        public WebClientOptions ClientApplications { get; set; }

        public AdminOptions Admin { get; set; }
    }
}
