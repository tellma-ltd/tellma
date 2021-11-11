using System.Collections.Generic;
using System.Linq;
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

        Task<AbstractPrintingTemplate> GetPrintingTemplate(int templateId, CancellationToken cancellation)
            => throw new ServiceException("Printing templates are not supported in this API.");

        Task SetPrintingVariables(Dictionary<string, EvaluationVariable> localVariables, Dictionary<string, EvaluationVariable> globalVariables, CancellationToken cancellation)
            => throw new ServiceException("Printing templates are not supported in this API.");

        Task SetPrintingFunctions(Dictionary<string, EvaluationFunction> localVariables, Dictionary<string, EvaluationFunction> globalVariables, CancellationToken cancellation)
            => throw new ServiceException("Printing templates are not supported in this API.");

        Task<AbstractEmailTemplate> GetEmailTemplate(int templateId, CancellationToken cancellation)
            => throw new ServiceException("Notification templates are not supported in this API.");

        void SetDefinitionId(int definitionId);

        Task<IEnumerable<AbstractPermission>> UserPermissions(string view, string action, CancellationToken cancellation);
    }

    public class AbstractPrintingTemplate
    {
        public AbstractPrintingTemplate(string body, string downloadName, string context, IEnumerable<AbstractParameter> parameters)
        {
            Body = body;
            DownloadName = downloadName;
            Context = context;

            Parameters = parameters?.ToList();
        }

        public string Body { get; }
        public string DownloadName { get; }
        public string Context { get; }

        public IEnumerable<AbstractParameter> Parameters { get; }
    }

    public class AbstractEmailTemplate
    {
        public AbstractEmailTemplate(
            string listExpression, 
            string subjectTemplate, 
            string bodyTemplate,
            IEnumerable<AbstractEmailRecipient> recipients,
            IEnumerable<AbstractParameter> parameters,
            IEnumerable<AbstractPrintingTemplate> attachments)
        {
            ListExpression = listExpression;
            SubjectTemplate = subjectTemplate;
            BodyTemplate = bodyTemplate;
            Recipients = recipients?.ToList();
            Parameters = parameters?.ToList();
            Attachments = attachments?.ToList();
        }

        public string ListExpression { get; }
        public string SubjectTemplate { get; }
        public string BodyTemplate { get; }
        public IEnumerable<AbstractEmailRecipient> Recipients { get; }
        public IEnumerable<AbstractParameter> Parameters { get; }
        public IEnumerable<AbstractPrintingTemplate> Attachments { get; }
        public IEnumerable<AbstractEmailRecipient> RegularRecipients => Recipients?.Where(e => e.Kind == RecipientKind.Regular) ?? Enumerable.Empty<AbstractEmailRecipient>();
        public IEnumerable<AbstractEmailRecipient> CcRecipients => Recipients?.Where(e => e.Kind == RecipientKind.Cc) ?? Enumerable.Empty<AbstractEmailRecipient>();
        public IEnumerable<AbstractEmailRecipient> BccRecipients => Recipients?.Where(e => e.Kind == RecipientKind.Bcc) ?? Enumerable.Empty<AbstractEmailRecipient>();
    }

    public class AbstractEmailRecipient
    {
        public AbstractEmailRecipient(string template, RecipientKind kind = RecipientKind.Regular)
        {
            Template = template;
            Kind = kind;
        }

        public string Template { get; }
        public RecipientKind Kind { get; }
    }

    public enum RecipientKind { Regular, Cc, Bcc }

    public class AbstractParameter
    {
        public AbstractParameter(string key, string control)
        {
            Key = key;
            Control = control;
        }

        public string Key { get; }
        public string Control { get; }
    }
}
