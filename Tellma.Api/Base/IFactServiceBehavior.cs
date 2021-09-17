using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Metadata;
using Tellma.Api.Templating;
using Tellma.Model.Common;
using Tellma.Repository.Common;

namespace Tellma.Api.Base
{
    public interface IFactServiceBehavior : IServiceBehavior
    {
        IQueryFactory QueryFactory<TEntity>() where TEntity : Entity;

        Task<IMetadataOverridesProvider> GetMetadataOverridesProvider(CancellationToken cancellation) 
            => Task.FromResult<IMetadataOverridesProvider>(new NullMetadataOverridesProvider());

        Task<AbstractPrintingTemplate> GetPrintingTemplate<TEntity>(int templateId, CancellationToken cancellation) where TEntity : Entity
            => throw new ServiceException("Printing templates are not supported in this API.");

        Task SetPrintingVariables(Dictionary<string, EvaluationVariable> localVariables, Dictionary<string, EvaluationVariable> globalVariables, CancellationToken cancellation)
            => throw new ServiceException("Printing templates are not supported in this API.");

        Task SetPrintingFunctions(Dictionary<string, EvaluationFunction> localVariables, Dictionary<string, EvaluationFunction> globalVariables, CancellationToken cancellation)
            => throw new ServiceException("Printing templates are not supported in this API.");

        void SetDefinitionId(int definitionId);

        Task<IEnumerable<AbstractPermission>> UserPermissions(string view, string action, CancellationToken cancellation);
    }

    public class AbstractPrintingTemplate
    {
        public AbstractPrintingTemplate(string body, string downloadName, string markupLanguage, string context)
        {
            Body = body;
            DownloadName = downloadName;
            MarkupLanguage = markupLanguage;
            Context = context;
        }

        public string Body { get; }
        public string DownloadName { get; }
        public string MarkupLanguage { get; }
        public string Context { get; }
    }
}
