namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// Indicates that the ZATCA server has encountered an unidentified internal error (StatusCode=500).
    /// </summary>
    public class ZatcaInternalException : ZatcaException
    {
        public ZatcaInternalException(string msg) : base(msg, isTransient: true) // Hopefully transient
        {
        }
    }
}
