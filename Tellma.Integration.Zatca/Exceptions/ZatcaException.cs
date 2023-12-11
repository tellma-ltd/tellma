using System.Net;

namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// Base class for all exceptions returned by the ZATCA API.
    /// </summary>
    public class ZatcaException : Exception
    {
        public ZatcaException(string msg, bool isTransient) : base(msg)
        {
            IsTransient = isTransient;
        }

        public bool IsTransient { get; }
    }
}
