using System;

namespace BSharp.Services.MultiTenancy
{
    public class MultitenancyException : Exception
    {
        public MultitenancyException(string message) : base(message)
        {
        }
    }
}
