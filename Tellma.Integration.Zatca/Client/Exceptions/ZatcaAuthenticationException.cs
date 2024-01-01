namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// Indicates that the request credentials were invalid.
    /// </summary>
    public class ZatcaAuthenticationException : ZatcaException
    {
        public ZatcaAuthenticationException() : base("The ZATCA credentials are not valid.", isTransient: false)
        {
        }
    }
}
