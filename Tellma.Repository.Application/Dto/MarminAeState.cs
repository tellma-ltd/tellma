namespace Tellma.Repository.Application
{
    /// <summary>
    /// The lifecycle of a document on the Marmin UAE / Peppol network, stored in
    /// <c>dbo.Documents.MarminAeState</c>.
    /// </summary>
    /// <remarks>
    /// The ordering matters as much as the values. <c>bll.Documents_Validate__Open</c> blocks
    /// reopening at <c>MarminAeState &gt;= 1</c>, which is why <see cref="Submitting"/> is 0: a
    /// document that never reached the vendor must stay reopenable, or a failed submission would
    /// strand it permanently. Everything from <see cref="Submitted"/> up is on the network and
    /// must not be edited, even though Peppol has not necessarily answered yet.
    /// </remarks>
    public enum MarminAeState
    {
        /// <summary>
        /// Claimed for submission, but the vendor has not confirmed receipt. Set inside the close
        /// transaction so the document can never be picked up for submission twice.
        /// </summary>
        Submitting = 0,

        /// <summary>The vendor accepted the document. Peppol validation is still in progress.</summary>
        Submitted = 1,

        /// <summary>Peppol confirmed delivery. The terminal success state.</summary>
        Delivered = 10,

        /// <summary>The vendor refused the submission. The document never reached the network.</summary>
        SubmitFailed = -10,

        /// <summary>The vendor accepted it but Peppol validation or delivery then failed.</summary>
        PeppolRejected = -20,
    }
}
