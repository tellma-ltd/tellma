namespace Tellma.Services.Sms
{
    public class TwilioOptions
    {
        public string AccountSid { get; set; }

        public string AuthToken { get; set; }
     
        public SmsOptions Sms { get; set; } = new SmsOptions();
    }
}
