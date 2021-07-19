using Tellma.Api.Base;
using Tellma.Api.Behaviors;

namespace Tellma.Api
{
    /// <summary>
    /// All the dependencies required by the <see cref="ApplicationSettingsServiceBase{TSettingsForSave, TSettings}"/> base class.
    /// This is a convenience for the developer so that all the dependencices do not have to 
    /// be listed in the constructor every single time a new inheriting class is implemented.
    /// </summary>
    public class ApplicationSettingsServiceDependencies
    {
        public IServiceContextAccessor Context { get; }

        public ISettingsCache SettingsCache { get; }

        public ApplicationVersions Versions { get; }
    }
}
