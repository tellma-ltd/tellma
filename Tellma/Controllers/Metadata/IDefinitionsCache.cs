using Tellma.Controllers.Dto;

namespace Tellma.Controllers
{
    /// <summary>
    /// Some entities have their metadata dynamically constructed from database configuration (settings, definitions).
    /// This service caches the metadata in memory since it is frequently queried
    /// </summary>
    public interface IDefinitionsCache
    {


        /// <summary>
        /// Returns the cached model metadata associated with the given database ID or null if non exist.
        /// Once this it is called, the result is guaranteed to be be the same for any subsequent calls
        /// within the same request scope, unless parameter forceFresh = true
        /// </summary>
        Versioned<DefinitionsForClient> GetDefinitionsIfCached(int databaseId, bool forceFresh = false);

        /// <summary>
        /// Returns the cached model metadata associated with the given database ID or null if non exist.
        /// Once this it is called, the result is guaranteed to be be the same for any subsequent calls
        /// within the same request scope, unless parameter forceFresh = true
        /// </summary>
        Versioned<DefinitionsForClient> GetCurrentDefinitionsIfCached(bool forceFresh = false);

        /// <summary>
        /// Sets the cached model metadata associated with the given database ID
        /// </summary>
        void SetDefinitions(int databaseId, Versioned<DefinitionsForClient> modelMetadata);
    }
}
