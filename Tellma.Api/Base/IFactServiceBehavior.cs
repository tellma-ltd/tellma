using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Api.Templating;
using Tellma.Model.Common;
using Tellma.Repository.Common;

namespace Tellma.Api.Base
{
    public interface IFactServiceBehavior : IServiceBehavior
    {
        IQueryFactory QueryFactory<TEntity>() where TEntity : Entity;

        Task<AbstractMarkupTemplate> GetMarkupTemplate<TEntity>(int templateId) where TEntity : Entity;

        Task SetMarkupVariables(Dictionary<string, EvaluationVariable> localVariables, Dictionary<string, EvaluationVariable> globalVariables);

        Task SetMarkupFunctions(Dictionary<string, EvaluationFunction> localVariables, Dictionary<string, EvaluationFunction> globalVariables);

        void SetDefinitionId(int definitionId);
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
