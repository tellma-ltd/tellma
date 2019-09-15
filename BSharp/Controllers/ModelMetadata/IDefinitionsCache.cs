using BSharp.Controllers.Dto;

namespace BSharp.Controllers
{
    /// <summary>
    /// Some entities have their metadata dynamically constructed from database configuration (Settings, definitions).
    /// This service caches the metadata in memory since it is frequently queried
    /// </summary>
    public interface IDefinitionsCache
    {
        /// <summary>
        /// Returns the cached model metadata associated with the given database ID or null if non exist
        /// </summary>
        DataWithVersion<DefinitionsForClient> GetDefinitionsIfCached(int databaseId);


        /// <summary>
        /// Returns the cached model metadata associated with the given database ID or null if non exist
        /// </summary>
        DataWithVersion<DefinitionsForClient> GetCurrentDefinitionsIfCached();

        /// <summary>
        /// Sets the cached model metadata associated with the given database ID
        /// </summary>
        void SetDefinitions(int databaseId, DataWithVersion<DefinitionsForClient> modelMetadata);
    }
}
