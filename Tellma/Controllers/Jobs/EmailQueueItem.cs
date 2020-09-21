namespace Tellma.Controllers.Jobs
{
    public class EmailQueueItem
    {
        /// <summary>
        /// The email address to send the email to
        /// </summary>
        public string ToEmail { get; set; }

        /// <summary>
        /// The email address to send the email to
        /// </summary>
        public string FromEmail { get; set; }

        /// <summary>
        /// The subject (title) of the email
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// The content of the email
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// The email id in the tenant database
        /// </summary>
        public int EmailId { get; set; }

        /// <summary>
        /// The Id of the tenant where the email is stored
        /// </summary>
        public int TenantId { get; set; }
    }
}
