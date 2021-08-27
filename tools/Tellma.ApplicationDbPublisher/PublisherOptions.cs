namespace Tellma.ApplicationDbPublisher
{
    /// <summary>
    /// Options for the application DB publisher.
    /// </summary>
    public class PublisherOptions
    {
        /// <summary>
        /// The connection string to the admin database containing the catalogue of all the application databases.
        /// </summary>
        /// <remarks>
        /// This parameter is required.
        /// </remarks>
        public string AdminConnection { get; set; }

        /// <summary>
        /// The path to the DACPAC file to publish.
        /// </summary>
        /// <remarks>
        /// This parameter is required.
        /// </remarks>
        public string DacpacFile { get; set; }

        /// <summary>
        /// The path to the folder where the backups are persisted.
        /// </summary>
        public string BackupFolder { get; set; }

        /// <summary>
        /// When false (default) the publisher extracts a backup for every database before publishing the DACPAC.
        /// </summary>
        public bool SkipBackup { get; set; }

        /// <summary>
        /// When false (default) the publisher confirms with the user before publishing the DACPAC.
        /// </summary>
        public bool SkipConfirmation { get; set; }

        /// <summary>
        /// The number of publish operations to perform in parallel.
        /// </summary>
        public int BatchSize { get; set; } = 100;
    }
}
