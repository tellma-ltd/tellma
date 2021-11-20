namespace Tellma.Api
{
    public class NotifyDocumentAssignmentArguments
    {
        /// <summary>
        /// The definition Id of the assigned documents.
        /// </summary>
        public int DefinitionId { get; set; }

        /// <summary>
        /// The singular title of the definition of the assigned documents in the preferred language of the assignee.
        /// </summary>
        public string SingularTitle { get; set; }

        /// <summary>
        /// The plural title of the definition of the assigned documents in the preferred language of the assignee.
        /// </summary>
        public string PluralTitle { get; set; }

        /// <summary>
        /// The total count of the assigned documents.
        /// </summary>
        public int DocumentCount { get; set; }

        /// <summary>
        /// The Id of the first assigned document. This is ignored if <see cref="DocumentCount"/> is more than 1.
        /// </summary>
        public int DocumentId { get; set; }

        /// <summary>
        /// The formatted serial number of the first assigned document. This is ignored if <see cref="DocumentCount"/> is more than 1.
        /// </summary>
        public string FormattedSerial { get; set; }

        /// <summary>
        /// The name of the sender in the preferred language of the assignee.
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// An optional comment supplied by the assigner.
        /// </summary>
        public string SenderComment { get; set; }
    }
}
