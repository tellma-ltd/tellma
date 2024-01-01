namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// The response status from the ZATCA API. <br/>
    /// The values were enumerated from <see href="https://sandbox.zatca.gov.sa/IntegrationSandbox">ZATCA Integration Sandbox</see>.
    /// </summary>
    public enum ResponseStatus
    {
        /// <summary>
        /// The operation was successful.
        /// </summary>
        Success = 200,

        /// <summary>
        /// The operation was successful but with warnings.
        /// </summary>
        SuccessWithWarnings = 202,

        /// <summary>
        /// The clearance API is currently deactivated.
        /// </summary>
        ClearanceDeactivated = 303,

        /// <summary>
        /// The operation failed due to invalid input.
        /// </summary>
        InvalidRequest = 400,

        /// <summary>
        /// The operation failed due to invalid credentials.
        /// </summary>
        InvalidCredentials = 401,

        /// <summary>
        /// The operation failed due to missing or unsupported version.
        /// </summary>
        InvalidVersion = 406,

        /// <summary>
        /// The operation failed due to an unkown internal server error.
        /// </summary>
        ServerError = 500,
    }
}