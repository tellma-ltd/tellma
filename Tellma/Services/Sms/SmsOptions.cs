namespace Tellma.Services.Sms
{
    public class SmsOptions
    {
        /// <summary>
        /// Notify Service Sid
        /// </summary>
        public string ServiceSid { get; set; }

        /// <summary>
        /// True if Twilio webhooks are enabled
        /// </summary>
        public bool CallbacksEnabled { get; set; }

        /// <summary>
        /// The name on which the system is hosted, Twilio will send SMS events it
        /// </summary>
        public string CallbackHost { get; set; }
    }
}
