using Microsoft.Extensions.Localization;
using Tellma.Api.Metadata;
using Tellma.Api.Templating;

namespace Tellma.Api.Base
{
    public class ServiceDependencies
    {
        public ServiceDependencies(IStringLocalizer<Strings> localizer,  MetadataProvider metadata, TemplateService templateService)
        {
            Localizer = localizer;
            Metadata = metadata;
            TemplateService = templateService;
        }

        public IStringLocalizer<Strings> Localizer { get; }
        public MetadataProvider Metadata { get; }
        public TemplateService TemplateService { get; }
    }
}
