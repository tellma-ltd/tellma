namespace Tellma.BlobsMigrator
{
    public class MigratorOptions
    {
        /// <summary>
        /// The connection string to the admin database containing the catalogue of all the application databases.
        /// </summary>
        /// <remarks>
        /// This parameter is required.
        /// </remarks>
        public string AdminConnection { get; set; }

        public string BlobStorageConnectionString { get; set; }

        public string BlobStorageContainerName { get; set; }

        public int TenantId { get; set; }
    }
}
