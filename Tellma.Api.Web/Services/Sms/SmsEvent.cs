namespace Tellma.Services.Sms
{
    public enum SmsEvent
    {
        /// <summary>
        /// External service sent it to network carrier
        /// </summary>
        Sent,

        /// <summary>
        /// External service failed to send it to network carrier
        /// </summary>
        Failed,

        /// <summary>
        /// Network carrier delivered it to end user
        /// </summary>
        Delivered,

        /// <summary>
        /// Network carrier failed to deliver it to end user
        /// </summary>
        Undelivered
    }
}
