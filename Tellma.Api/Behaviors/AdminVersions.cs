namespace Tellma.Api.Behaviors
{
    /// <summary>
    /// Stores in a scoped object the cache versions that were loaded from the 
    /// database inside <see cref="AdminServiceBehavior.OnInitialize"/>.
    /// </summary>
    public class AdminVersions
    {
        public bool AreSet { get; internal set; }
        public string PermissionsVersion { get; internal set; }
        public string UserSettingsVersion { get; internal set; }
    }
}
