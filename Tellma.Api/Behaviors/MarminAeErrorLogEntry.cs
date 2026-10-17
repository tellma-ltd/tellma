using Tellma.Api.Behaviors;

namespace Tellma.Api
{
    /// <summary>
    /// Signifies a failure or a warning while submitting a document to the Marmin UAE
    /// e-invoicing API. Delivered to the tenant's support emails.
    /// </summary>
    /// <remarks>
    /// Mirrors <see cref="ZatcaErrorLogEntry"/>, minus the invoice XML: Tellma sends JSON to
    /// Marmin and never builds the UBL itself, so there is no document of ours to attach.
    /// </remarks>
    public class MarminAeErrorLogEntry(TenantLogLevel level) : TenantLogEntry(level)
    {
        /// <summary>
        /// The Id of the document definition that caused the problem
        /// </summary>
        public int DocumentDefinitionId { get; set; }

        /// <summary>
        /// The name of the document definition that caused the problem
        /// </summary>
        public string DefinitionName { get; set; }

        /// <summary>
        /// The document that was closed
        /// </summary>
        public int DocumentId { get; set; }

        /// <summary>
        /// The document's code, which is also the document_number sent to the vendor. This is what
        /// an administrator searches for in the Marmin portal.
        /// </summary>
        public string DocumentNumber { get; set; }

        /// <summary>
        /// The error or warning, as reported by the vendor or by the integration.
        /// </summary>
        public string MarminAeResponseBody { get; set; }

        /// <summary>
        /// The email of the user who triggered the error.
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        /// The username of the user who triggered the error.
        /// </summary>
        public string UserName { get; set; }
    }
}
