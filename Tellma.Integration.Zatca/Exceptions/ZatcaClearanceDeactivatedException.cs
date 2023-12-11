namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// Indicates that the ZATCA clearance API is currently unavailable, requiring fallback to the reporting API.
    /// </summary>
    public class ZatcaClearanceDeactivatedException : ZatcaException
    {
        public ZatcaClearanceDeactivatedException() : 
            base(msg: "Clearance is deactiviated. Please use the Reporting endpoint instead.", isTransient: false)
        {
        }
    }
}
