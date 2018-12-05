namespace BSharp.Controllers.Shared
{
    /// <summary>
    /// The base class for all DTOs of HTTP resources that can be modified
    /// </summary>
    public abstract class DtoForSaveKeyBase<TKey> : DtoForSaveBase
    {
        /// <summary>
        /// This is an integer for entities that have a simple integer key in the SQL database,
        /// and a string for anything else (The string can encode a composite keys for example) 
        /// it is important to have a single Id property for tracking HTTP resources as it simplifies
        /// so much shared logic for tracking resources and caching them
        /// </summary>
        public TKey Id { get; set; }

        /// <summary>
        /// Either 'Insert' or 'Update' or null
        /// </summary>
        [ChoiceList("Inserted", "Updated", "Deleted")]
        public string EntityState { get; set; }
    }
}
