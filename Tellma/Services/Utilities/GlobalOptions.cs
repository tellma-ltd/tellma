namespace Tellma.Services.Utilities
{
    public class GlobalOptions
    {
        public bool EmailEnabled { get; set; } = true;

        public bool EmbeddedIdentityServerEnabled { get; set; }

        public bool EmbeddedClientApplicationEnabled { get; set; }

        public bool InstrumentationEnabled { get; set; }

        public LocalizationOptions Localization { get; set; }

        public WebClientOptions ClientApplications { get; set; }

        public AdminOptions Admin { get; set; }
    }
}
