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
        public string ConnectionString { get; set; }

        /// <summary>
        /// The name of the source database, e.g. Tellma.101.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The name of the destination database.
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// The URL to launch when done.
        /// </summary>
        public string LaunchUrl { get; set; }
    }
}
