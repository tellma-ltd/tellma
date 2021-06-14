namespace Tellma.Data
{
    /// <summary>
    /// Database operations that alter the number of assigned documents per user, will return a list
    /// of these objects to indicate which users need to be notified of a changed assignments count
    /// </summary>
    public class InboxNotificationInfo
    {
        /// <summary>
        /// The user to notify
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// The new total count of assigned documents
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The new total count of unkown assigned documents
        /// </summary>
        public int UnknownCount { get; set; }
    }
}
