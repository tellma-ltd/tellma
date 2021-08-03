namespace Tellma.Controllers.Dto
{
    public class GlobalSettingsForClient
    {
        public bool EmailEnabled { get; set; }
        public bool SmsEnabled { get; set; }
        public bool PushEnabled { get; set; }

        public bool CanInviteUsers { get; set; }
        public int TokenExpiryInDays { get; set; }
    }
}
