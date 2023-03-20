namespace Tellma.ApplicationDbCloner
{
    public class ClonerOptions
    {
        /// <summary>
        /// The connection string to the admin database containing the catalogue of all the application databases.
        /// </summary>
        /// <remarks>
        /// This parameter is required.
        /// </remarks>
        public string AdminConnection { get; set; }

        /// <summary>
        /// The Id of the source database, e.g. 101.
        /// </summary>
        public int SourceId { get; set; }

        /// <summary>
        /// The Id of the destination database, e.g. 1101.
        /// </summary>
        public int DestinationId { get; set; }

        /// <summary>
        /// If true, does not add the new company to the memberships of the users.
        /// </summary>
        public bool SkipDirectoryUserMemberships { get; set; }
    }
}
