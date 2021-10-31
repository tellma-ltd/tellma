namespace Tellma.Utilities.Email
{
    /// <summary>
    /// A DTO of an attachment that comes with <see cref="EmailToSend"/>.
    /// </summary>
    public class EmailAttachmentToSend
    {
        /// <summary>
        /// The name of the attachment file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The byte content of the attachment file.
        /// </summary>
        public byte[] Contents { get; set; }
    }
}
