using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Metadata;

namespace Tellma.Api
{
    /// <summary>
    /// All the dependencies required by the <see cref="ApplicationSettingsServiceBase{TSettingsForSave, TSettings}"/> base class.
    /// This is a convenience for the developer so that all the dependencices do not have to 
    /// be listed in the constructor every single time a new inheriting class is implemented.
    /// </summary>
    public class ApplicationSettingsServiceDependencies
    {
        public ApplicationSettingsServiceDependencies(
            IServiceContextAccessor context,
            ISettingsCache settingsCache, 
            IPermissionsCache permissionsCache,
            ApplicationServiceBehavior behavior,
            MetadataProvider metadataProvider)
        {
            Context = context;
            SettingsCache = settingsCache;
            PermissionsCache = permissionsCache;
            Behavior = behavior;
            MetadataProvider = metadataProvider;
        }

        public IServiceContextAccessor Context { get; }

        public ISettingsCache SettingsCache { get; }

        public IPermissionsCache PermissionsCache { get; }

        public ApplicationServiceBehavior Behavior { get; }

        public MetadataProvider MetadataProvider { get; }
    }
}
