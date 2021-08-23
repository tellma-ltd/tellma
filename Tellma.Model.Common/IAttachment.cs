namespace Tellma.Model.Common
{
    /// <summary>
    /// Standardizes entities that represent attachments.
    /// </summary>
    public interface IAttachment
    {
        int Id { get; set; }

        byte[] File { get; set; }

        public EntityMetadata EntityMetadata { get; set; }
    }
}
