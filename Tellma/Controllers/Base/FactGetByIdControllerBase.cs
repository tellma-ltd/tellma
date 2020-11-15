using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Templating;
using Tellma.Controllers.Utilities;
using Tellma.Data.Queries;
using Tellma.Entities;

namespace Tellma.Controllers
{
    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using OData-like parameters
    /// and allow selecting a certain record by Id
    /// </summary>
    public abstract class FactGetByIdControllerBase<TEntity, TKey> : FactWithIdControllerBase<TEntity, TKey>
        where TEntity : EntityWithKey<TKey>
    {
        public FactGetByIdControllerBase(IServiceProvider sp) : base(sp)
        {
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<GetByIdResponse<TEntity>>> GetById(TKey id, [FromQuery] GetByIdArguments args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Calculate server time at the very beginning for consistency
                var serverTime = DateTimeOffset.UtcNow;

                // Load the data
                var service = GetFactGetByIdService();
                var (entity, extras) = await service.GetById(id, args, cancellation);

                // Load the extras
                var singleton = new List<TEntity> { entity };

                // Flatten and Trim
                var relatedEntities = FlattenAndTrim(singleton, cancellation);

                // Prepare the result in a response object
                var result = new GetByIdResponse<TEntity>
                {
                    Result = entity,
                    RelatedEntities = relatedEntities,
                    CollectionName = ControllerUtilities.GetCollectionName(typeof(TEntity)),
                    Extras = TransformExtras(extras, cancellation),
                    ServerTime = serverTime,
                };
                return Ok(result);
            }, _logger);
        }


        [HttpGet("{id}/print/{templateId}")]
        public async Task<ActionResult> PrintById(TKey id, int templateId, [FromQuery] GenerateMarkupByIdArguments args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var service = GetFactGetByIdService();
                var (fileBytes, fileName) = await service.PrintById(id, templateId, args, cancellation);
                var contentType = ControllerUtilities.ContentType(fileName);
                return File(fileContents: fileBytes, contentType: contentType, fileName);
            }, _logger);
        }

        protected override FactWithIdServiceBase<TEntity, TKey> GetFactWithIdService()
        {
            return GetFactGetByIdService();
        }

        protected abstract FactGetByIdServiceBase<TEntity, TKey> GetFactGetByIdService();
    }

    public abstract class FactGetByIdServiceBase<TEntity, TKey> : FactWithIdServiceBase<TEntity, TKey>, IFactGetByIdServiceBase
        where TEntity : EntityWithKey<TKey>
    {
        // Private Fields
        public FactGetByIdServiceBase(IServiceProvider sp) : base(sp)
        {
        }

        /// <summary>
        /// Returns a <see cref="TEntity"/> as per the Id and the specifications in the <see cref="GetByIdArguments"/>, after verifying the user's permissions
        /// </summary>
        public virtual async Task<(TEntity, Extras)> GetById(TKey id, GetByIdArguments args, CancellationToken cancellation)
        {
            // Parse the parameters
            var expand = ExpandExpression.Parse(args?.Expand);
            var select = ParseSelect(args?.Select);

            // Load the data
            var data = await GetEntitiesByIds(new List<TKey> { id }, expand, select, null,  cancellation);

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

        public async Task<(byte[] FileBytes, string FileName)> PrintById(TKey id, int templateId, [FromQuery] GenerateMarkupArguments args, CancellationToken cancellation)
        {
            var collection = ControllerUtilities.GetCollectionName(typeof(TEntity));
            var defId = DefinitionId;
            var repo = GetRepository();

            var template = await repo.Query<MarkupTemplate>().FilterByIds(new int[] { templateId }).FirstOrDefaultAsync(cancellation);
            if (template == null)
            {
                // Shouldn't happen in theory cause of previous check, but just to be extra safe
                throw new BadRequestException($"The template with Id {templateId} does not exist");
            }

            if (!(template.IsDeployed ?? false))
            {
                // A proper UI will only allow the user to use supported template
                throw new BadRequestException($"The template with Id {templateId} is not deployed");
            }

            // The errors below should be prevented through SQL validation, but just to be safe
            if (template.Usage != MarkupTemplateConst.QueryById)
            {
                throw new BadRequestException($"The template with Id {templateId} does not have the proper usage");
            }

            if (template.MarkupLanguage != MimeTypes.Html)
            {
                throw new BadRequestException($"The template with Id {templateId} is not an HTML template");
            }

            if (template.Collection != collection)
            {
                throw new BadRequestException($"The template with Id {templateId} does not have Collection = '{collection}'");
            }

            if (template.DefinitionId != null && template.DefinitionId != defId)
            {
                throw new BadRequestException($"The template with Id {templateId} does not have DefinitionId = '{defId}'");
            }

            // Onto the printing itself

            var templates = new string[] { template.DownloadName, template.Body };
            var tenantInfo = _tenantInfo.GetCurrentInfo();
            var culture = TemplateUtil.GetCulture(args, tenantInfo);

            var preloadedQuery = new QueryByIdInfo(collection, defId, id.ToString());
            var inputVariables = new Dictionary<string, object>
            {
                ["$Source"] = $"{collection}/{defId}",
                ["$Id"] = id
            };

            // Generate the output
            string[] outputs;
            try
            {
                outputs = await _templateService.GenerateMarkup(templates, inputVariables, preloadedQuery, culture, cancellation);
            }
            catch (TemplateException ex)
            {
                throw new BadRequestException(ex.Message);
            }

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
                if(int.TryParse(stringId, out int intId))
                {
                    id = intId;
                    return await GetById((TKey)id, args, cancellation);
                } 
                else
                {
                    throw new BadRequestException($"Value '{id}' could not be interpreted as a valid integer");
                }
            } 
            else
            {
                throw new InvalidOperationException("Bug: Only integer and string Ids are supported");
            }

        }
    }

    public interface IFactGetByIdServiceBase : IFactWithIdService
    {
        Task<(EntityWithKey, Extras)> GetById(object id, GetByIdArguments args, CancellationToken cancellation);
    }
}
