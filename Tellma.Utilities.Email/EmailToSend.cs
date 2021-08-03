namespace Tellma.Utilities.Email
{
    /// <summary>
    /// A DTO of an Email.
    /// </summary>
    public class EmailToSend
    {
        public EmailToSend(string toEmail)
        {
            ToEmail = toEmail;
        }

        /// <summary>
        /// The email address to send the email to.
        /// </summary>
        public string ToEmail { get; }

        /// <summary>
        /// The subject (title) of the email.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// The content of the email.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// The email id in the tenant database or 0 if there is no tenant Id.
        /// </summary>
        public int EmailId { get; set; }

        /// <summary>
        /// The Id of the tenant where the email is stored or 0 if there is no tenant Id.
        /// </summary>
        public int TenantId { get; set; }
    }
}
