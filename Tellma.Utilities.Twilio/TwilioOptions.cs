namespace Tellma.Utilities.Twilio
{
    public class TwilioOptions
    {
        public string AccountSid { get; set; }

        public string AuthToken { get; set; }
     
        public TwilioSmsOptions Sms { get; set; } = new();
    }
}
