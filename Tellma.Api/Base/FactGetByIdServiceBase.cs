using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Api.Templating;
using Tellma.Model.Common;
using Tellma.Repository.Common;
using Tellma.Utilities.Common;

namespace Tellma.Api.Base
{
    /// <summary>
    /// Services inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using OData-like parameters
    /// and allow selecting a certain record by Id.
    /// </summary>
    public abstract class FactGetByIdServiceBase<TEntity, TKey> : FactWithIdServiceBase<TEntity, TKey>, IFactGetByIdServiceBase
        where TEntity : EntityWithKey<TKey>
    {
        #region Lifecycle

        private readonly TemplateService _templateService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FactGetByIdServiceBase{TEntity, TKey}"/> class.
        /// </summary>
        /// <param name="deps">The service dependencies.</param>
        public FactGetByIdServiceBase(FactServiceDependencies deps) : base(deps)
        {
            _templateService = deps.TemplateService;
        }

        /// <summary>
        /// Sets the definition Id that scopes the service to only a subset of the definitioned entities.
        /// </summary>
        public new FactGetByIdServiceBase<TEntity, TKey> SetDefinitionId(int definitionId)
        {
            base.SetDefinitionId(definitionId);
            return this;
        }

        #endregion

        #region API

        /// <summary>
        /// Returns a the single entity of type <see cref="TEntity"/> which has the given <paramref name="id"/> 
        /// according the specifications in the <paramref name="args"/>, after verifying the user's permissions.
        /// </summary>
        /// <exception cref="NotFoundException{TKey}">If the entity is not found.</exception>
        public virtual async Task<(TEntity, Extras)> GetById(TKey id, GetByIdArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // Parse the parameters
            var expand = ExpressionExpand.Parse(args?.Expand);
            var select = ParseSelect(args?.Select);

            // Load the data
            var data = await GetEntitiesByIds(new List<TKey> { id }, expand, select, null, cancellation);

            // Check that the entity exists, else return NotFound
            var entity = data.SingleOrDefault();
            if (entity == null)
            {
                throw new NotFoundException<TKey>(id);
            }

            // Load the extras
            var extras = await GetExtras(data, cancellation);

            // Return
            return (entity, extras);
        }

        /// <summary>
        /// Returns a generated markup text file that is evaluated based on the given <paramref name="templateId"/>.
        /// The markup generation will implicitly contain a variable $ that evaluates to the entity whose id matches <paramref name="id"/>.
        /// </summary>
        public async Task<(byte[] FileBytes, string FileName)> PrintById(TKey id, int templateId, PrintEntityByIdArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // (1) Preloaded Query
            var collection = typeof(TEntity).Name;
            var defId = DefinitionId;

            QueryInfo preloadedQuery = new QueryEntityByIdInfo(
                    collection: collection,
                    definitionId: defId,
                    id: id);

            // (2) The templates
            var template = await FactBehavior.GetMarkupTemplate<TEntity>(templateId, cancellation);
            var templates = new (string, string)[] {
                (template.DownloadName, MimeTypes.Text),
                (template.Body, template.MarkupLanguage)
            };

            // (3) Functions + Variables
            var globalFunctions = new Dictionary<string, EvaluationFunction>();
            var localFunctions = new Dictionary<string, EvaluationFunction>();
            var globalVariables = new Dictionary<string, EvaluationVariable>();
            var localVariables = new Dictionary<string, EvaluationVariable>
            {
                ["$Source"] = new EvaluationVariable($"{collection}/{defId}"),
                ["$Id"] = new EvaluationVariable(id),
            };

            await FactBehavior.SetMarkupFunctions(localFunctions, globalFunctions, cancellation);
            await FactBehavior.SetMarkupVariables(localVariables, globalVariables, cancellation);

            // (4) Culture
            CultureInfo culture = GetCulture(args.Culture);

            // Generate the output
            var genArgs = new MarkupArguments(templates, globalFunctions, globalVariables, localFunctions, localVariables, preloadedQuery, culture);
            string[] outputs = await _templateService.GenerateMarkup(genArgs, cancellation);

            var downloadName = outputs[0];
            var body = outputs[1];

            // Change the body to bytes
            var bodyBytes = Encoding.UTF8.GetBytes(body);

            // Do some sanitization of the downloadName
            if (string.IsNullOrWhiteSpace(downloadName))
            {
                downloadName = id.ToString();
            }

            if (!downloadName.ToLower().EndsWith(".html"))
            {
                downloadName += ".html";
            }

            // Return as a file
            return (bodyBytes, downloadName);
        }

        #endregion

        #region IFactGetByIdServiceBase

        async Task<(EntityWithKey, Extras)> IFactGetByIdServiceBase.GetById(object id, GetByIdArguments args, CancellationToken cancellation)
        {
            Type target = typeof(TKey);
            if (target == typeof(string))
            {
                id = id?.ToString();
                return await GetById((TKey)id, args, cancellation);
            }
            else if (target == typeof(int) || target == typeof(int?))
            {
                string stringId = id?.ToString();
                if (int.TryParse(stringId, out int intId))
                {
                    id = intId;
                    return await GetById((TKey)id, args, cancellation);
                }
                else
                {
                    throw new ServiceException($"Value '{id}' could not be interpreted as a valid integer");
                }
            }
            else
            {
                throw new InvalidOperationException("Bug: Only integer and string Ids are supported");
            }
        }

        #endregion
    }

    public interface IFactGetByIdServiceBase : IFactWithIdService
    {
        Task<(EntityWithKey, Extras)> GetById(object id, GetByIdArguments args, CancellationToken cancellation);
    }
}
