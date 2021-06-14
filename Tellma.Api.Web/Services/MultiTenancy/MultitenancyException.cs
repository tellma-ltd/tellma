using System;

namespace Tellma.Services.MultiTenancy
{
    public class MultitenancyException : Exception
    {
        public MultitenancyException(string message) : base(message)
        {
        }
    }
}
