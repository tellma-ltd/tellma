using System;

namespace Tellma.Services.Email
{
    public class EmailApiException : Exception
    {
        public EmailApiException(string msg) : base(msg)
        {
        }
    }
}
