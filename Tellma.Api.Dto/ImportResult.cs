namespace Tellma.Api.Dto
{
    public class ImportResult
    {
        /// <summary>
        /// How many records were inserted.
        /// </summary>
        public int Inserted { get; set; }

        /// <summary>
        /// How many records were updated.
        /// </summary>
        public int Updated { get; set; }

        /// <summary>
        /// How long the import operation took.
        /// </summary>
        public long Milliseconds { get; set; }
    }
}
