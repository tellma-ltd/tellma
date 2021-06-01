using Microsoft.Extensions.Localization;
using Tellma.Api.Metadata;
using Tellma.Api.Templating;

namespace Tellma.Api.Base
{
    public class ServiceDependencies
    {
        public ServiceDependencies(IStringLocalizer<Strings> localizer,  MetadataProvider metadata, TemplateService templateService, IServiceContextAccessor contextAccessor)
        {
            Localizer = localizer;
            Metadata = metadata;
            TemplateService = templateService;
            ContextAccessor = contextAccessor;
        }

        public IStringLocalizer<Strings> Localizer { get; }
        public MetadataProvider Metadata { get; }
        public TemplateService TemplateService { get; }
        public IServiceContextAccessor ContextAccessor { get; set; }
    }
}
