namespace Tellma.Api.Dto
{
    public class ImportResult
    {
        public ImportResult(int inserted, int updated, long milliseconds)
        {
            Inserted = inserted;
            Updated = updated;
            Milliseconds = milliseconds;
        }

        /// <summary>
        /// How many records were inserted.
        /// </summary>
        public int Inserted { get; }

        /// <summary>
        /// How many records were updated.
        /// </summary>
        public int Updated { get; }

        /// <summary>
        /// How long the import operation took.
        /// </summary>
        public long Milliseconds { get; }
    }
}
