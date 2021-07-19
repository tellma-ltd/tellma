namespace Tellma.Utilities.Email
{
    /// <summary>
    /// A type of event that occurred to the email after it was sent.
    /// </summary>
    public enum EmailEvent
    {
        /// <summary>
        /// External service rejected it.
        /// </summary>
        Dropped,

        /// <summary>
        /// Recipient server accepted it.
        /// </summary>
        Delivered,

        /// <summary>
        /// Recipient server rejected it.
        /// </summary>
        Bounce,

        /// <summary>
        /// User opened the email.
        /// </summary>
        Open,

        /// <summary>
        /// User clicked a link in the email.
        /// </summary>
        Click,

        /// <summary>
        /// User marked email as spam.
        /// </summary>
        SpamReport
    }
}
