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

        Task<IMetadataOverridesProvider> GetMetadataOverridesProvider(CancellationToken cancellation);

        Task<AbstractMarkupTemplate> GetMarkupTemplate<TEntity>(int templateId, CancellationToken cancellation) where TEntity : Entity;

        Task SetMarkupVariables(Dictionary<string, EvaluationVariable> localVariables, Dictionary<string, EvaluationVariable> globalVariables, CancellationToken cancellation);

        Task SetMarkupFunctions(Dictionary<string, EvaluationFunction> localVariables, Dictionary<string, EvaluationFunction> globalVariables, CancellationToken cancellation);

        void SetDefinitionId(int definitionId);

        Task<IEnumerable<AbstractPermission>> UserPermissions(string view, string action, CancellationToken cancellation);
    }

    public class AbstractMarkupTemplate
    {
        public AbstractMarkupTemplate(string body, string downloadName, string markupLanguage)
        {
            Body = body;
            DownloadName = downloadName;
            MarkupLanguage = markupLanguage;
        }

        public string Body { get; set; }
        public string DownloadName { get; set; }
        public string MarkupLanguage { get; set; }
    }
}
