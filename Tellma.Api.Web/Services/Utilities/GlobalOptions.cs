namespace Tellma.Services.Utilities
{
    public class GlobalOptions
    {
        public bool EmailEnabled { get; set; } = true;

        public bool SmsEnabled { get; set; }

        public bool PushEnabled { get; set; } = true;

        public bool AzureBlobStorageEnabled { get; set; }

        public bool EmbeddedIdentityServerEnabled { get; set; }

        public bool EmbeddedClientApplicationEnabled { get; set; }
    }
}
