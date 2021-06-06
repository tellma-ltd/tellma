namespace Tellma.Api.Behaviors
{
    /// <summary>
    /// Stores in a scoped object the cache versions that were loaded from the 
    /// database inside <see cref="ApplicationServiceBehavior.OnInitialize"/>.
    /// </summary>
    public class ApplicationVersions
    {
        public string SettingsVersion { get; internal set; }
        public string DefinitionsVersion { get; internal set; }
        public string PermissionsVersion { get; internal set; }
        public string UserSettingsVersion { get; internal set; }
    }
}
