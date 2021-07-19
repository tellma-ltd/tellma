using System;

namespace Tellma.Utilities.Email
{
    public class EmailApiException : Exception
    {
        public EmailApiException(string msg) : base(msg)
        {
        }
    }
}
