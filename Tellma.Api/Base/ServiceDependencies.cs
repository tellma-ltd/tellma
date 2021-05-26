using Microsoft.Extensions.Localization;

namespace Tellma.Api
{
    public class ServiceDependencies
    {
        public ServiceDependencies(IStringLocalizer<Strings> localizer,  MetadataProvider metadata)
        {
            Localizer = localizer;
            Metadata = metadata;
        }

        public IStringLocalizer<Strings> Localizer { get; }
        public MetadataProvider Metadata { get; }
    }
}
