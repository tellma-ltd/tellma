using System;

namespace Tellma.Services.Sms
{
    /// <summary>
    /// Thrown by <see cref="ISmsSender"/> if the <see cref="SmsMessage"/> does not pass validation
    /// </summary>
    public class SmsInvalidException : Exception
    {
        public SmsInvalidException(string msg) : base(msg)
        {
        }
    }
}
