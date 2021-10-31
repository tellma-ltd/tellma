using Microsoft.Extensions.Localization;
using Tellma.Api.Metadata;
using Tellma.Api.Templating;
using Tellma.Utilities.Email;

namespace Tellma.Api.Base
{
    /// <summary>
    /// Packages all the dependencies of fact service base classes, so that inheriting classes
    /// do not have to list them all as constructor arguments and to simplify adding more 
    /// dependencies in the future. <br/>
    /// This is registered in the DI as scoped.
    /// </summary>
    public class FactServiceDependencies
    {
        public FactServiceDependencies(
            IStringLocalizer<Strings> localizer,  
            MetadataProvider metadata, 
            TemplateService templateService,
            IServiceContextAccessor contextAccessor,
            IEmailQueuer emailQueue)
        {
            Localizer = localizer;
            Metadata = metadata;
            TemplateService = templateService;
            ContextAccessor = contextAccessor;
            EmailQueue = emailQueue;
        }

        /// <summary>
        /// For localizing error messages and property names.
        /// </summary>
        public IStringLocalizer<Strings> Localizer { get; }

        /// <summary>
        /// A <see cref="MetadataProvider"/> for the entities.
        /// </summary>
        public MetadataProvider Metadata { get; }

        /// <summary>
        /// For printing.
        /// </summary>
        public TemplateService TemplateService { get; }

        /// <summary>
        /// For accessing contextual information that is universal per request.
        /// </summary>
        public IServiceContextAccessor ContextAccessor { get; }

        /// <summary>
        /// For sending template-based emails.
        /// </summary>
        public IEmailQueuer EmailQueue { get; }
    }
}
