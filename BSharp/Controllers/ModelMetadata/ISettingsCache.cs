using BSharp.Controllers.Dto;

namespace BSharp.Controllers
{
    /// <summary>
    /// Some entities have their metadata dynamically constructed from database configuration (settings, definitions).
    /// This service caches the metadata in memory since it is frequently queried
    /// </summary>
    public interface ISettingsCache
    {
        /// <summary>
        /// Returns the cached model metadata associated with the given database ID or null if non exist
        /// </summary>
        DataWithVersion<SettingsForClient> GetSettingsIfCached(int databaseId);

        /// <summary>
        /// Returns the cached model metadata associated with the given database ID or null if non exist
        /// </summary>
        DataWithVersion<SettingsForClient> GetCurrentSettingsIfCached();

        /// <summary>
        /// Sets the cached model metadata associated with the given database ID
        /// </summary>
        void SetSettings(int databaseId, DataWithVersion<SettingsForClient> modelMetadata);
    }
}
