namespace Tellma.Repository.Application
{
    /// <summary>
    /// The lifecycle of a document on the Marmin UAE / Peppol network, stored in
    /// <c>dbo.Documents.MarminAeState</c>. NULL means the document was never submitted.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Two thresholds read these values, so the numbers matter as much as the names.
    /// </para>
    /// <para>
    /// <b>Reopening</b> (<c>bll.Documents_Validate__Open</c>, Production only) is refused for
    /// anything that is, or may be, on the network: 1, 2, 10 and -30. Everything else is
    /// reopenable, because each has a remedy that starts with fixing the document.
    /// </para>
    /// <para>
    /// <b>Submitting</b> (<c>dal.MarminAe__GetInvoices</c>) picks up NULL, 0, -10 and -20 only,
    /// so a re-close or a resubmit never re-sends a document that is on the network.
    /// </para>
    /// </remarks>
    public enum MarminAeState
    {
        /// <summary>
        /// Claimed for submission, and certainly not sent. Set inside the close transaction; the
        /// document moves to <see cref="SentAwaitingOutcome"/> immediately before the HTTP call.
        /// </summary>
        Submitting = 0,

        /// <summary>The vendor accepted the document. Peppol is still processing it.</summary>
        Submitted = 1,

        /// <summary>
        /// The request to the vendor left, and its outcome is unknown: a timeout, a lost
        /// connection, or a crash before the outcome was recorded. It may be on the network, so
        /// it is treated as if it were. Refresh and Resubmit settle it by asking the vendor
        /// whether it holds a document with this document's number.
        /// </summary>
        SentAwaitingOutcome = 2,

        /// <summary>Peppol confirmed delivery (APPROVED). Final.</summary>
        Delivered = 10,

        /// <summary>
        /// The vendor refused the submission outright, so the document never reached the
        /// network. Fix it and resubmit.
        /// </summary>
        SubmitFailed = -10,

        /// <summary>
        /// The vendor accepted the document but Peppol validation failed (VALIDATION_FAILED).
        /// The vendor allows exactly this status to be resubmitted, with a PUT to the same id.
        /// </summary>
        PeppolValidationFailed = -20,

        /// <summary>
        /// Delivered and then rejected (REJECTED). Final, and not resubmittable: the vendor's
        /// remedy is a credit note against it and a new invoice.
        /// </summary>
        PeppolRejected = -30,
    }
}
