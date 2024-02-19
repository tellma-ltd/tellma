using Tellma.Api.Behaviors;

namespace Tellma.Api
{
    /// <summary>
    /// Signifies a bug that results in a ZATCA error/warning
    /// </summary>
    public class ZatcaErrorLogEntry(TenantLogLevel level) : TenantLogEntry(level)
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
        /// The XML invoice sent to ZATCA.
        /// </summary>
        public string InvoiceXml { get; set; }

        /// <summary>
        /// The error/warning response from ZATCA.
        /// </summary>
        public string ValidationResultsJson { get; set; }

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
