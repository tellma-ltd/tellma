using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Tellma.Controllers.Templating;
using System.Globalization;
using System.Text;
using Tellma.Services.Utilities;
using System;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class MarkupTemplatesController : CrudControllerBase<MarkupTemplateForSave, MarkupTemplate, int>
    {
        public const string BASE_ADDRESS = "markup-templates";

        private readonly MarkupTemplatesService _service;
        private readonly ILogger _logger;

        public MarkupTemplatesController(MarkupTemplatesService service, ILogger<MarkupTemplatesController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPut("preview-by-filter")]
        public async Task<ActionResult<MarkupPreviewResponse>> PreviewByFilter([FromBody] MarkupPreviewTemplate entity, [FromQuery] GenerateMarkupByFilterArguments args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var (body, downloadName) = await _service.PreviewByFilter(entity, args, cancellation);

                // Prepare and return the response
                var response = new MarkupPreviewResponse
                {
                    Body = body,
                    DownloadName = downloadName
                };

                return Ok(response);
            },
            _logger);
        }

        [HttpPut("preview-by-id/{id}")]
        public async Task<ActionResult<MarkupPreviewResponse>> PreviewById([FromRoute] string id, [FromBody] MarkupPreviewTemplate entity, [FromQuery] GenerateMarkupByIdArguments args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var (body, downloadName) = await _service.PreviewById(id, entity, args, cancellation);

                // Prepare and return the response
                var response = new MarkupPreviewResponse
                {
                    Body = body,
                    DownloadName = downloadName
                };

                return Ok(response);
            },
            _logger);
        }

        [HttpPut("preview")]
        public async Task<ActionResult<MarkupPreviewResponse>> Preview([FromBody] MarkupPreviewTemplate entity, [FromQuery] GenerateMarkupArguments args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var (body, downloadName) = await _service.Preview(entity, args, cancellation);

                // Prepare and return the response
                var response = new MarkupPreviewResponse
                {
                    Body = body,
                    DownloadName = downloadName
                };

                return Ok(response);
            },
            _logger);
        }

        protected override CrudServiceBase<MarkupTemplateForSave, MarkupTemplate, int> GetCrudService()
        {
            return _service;
        }
    }

    public class MarkupTemplatesService : CrudServiceBase<MarkupTemplateForSave, MarkupTemplate, int>
    {
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;
        private readonly TemplateService _templateService;

        private string View => MarkupTemplatesController.BASE_ADDRESS;

        public MarkupTemplatesService(
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo,
            TemplateService templateService,
            IServiceProvider sp) : base(sp)
        {
            _localizer = localizer;
            _repo = repo;
            _templateService = templateService;
        }

        public async Task<(string Body, string DownloadName)> Preview(MarkupPreviewTemplate entity, GenerateMarkupArguments args, CancellationToken cancellation)
        {
            // Everything to input in the template service
            var templates = new string[] { entity.DownloadName, entity.Body };
            var culture = TemplateUtil.GetCulture(args, await _repo.GetTenantInfoAsync(cancellation));

            // Generate the output
            var outputs = await _templateService.GenerateMarkup(templates, null, null, culture, cancellation);
            var downloadName = AppendExtension(outputs[0], entity);
            var body = outputs[1];

            // Return as a file
            return (body, downloadName);
        }

        public async Task<(string Body, string DownloadName)> PreviewByFilter(MarkupPreviewTemplate entity, GenerateMarkupByFilterArguments args, CancellationToken cancellation)
        {
            // Everything to input in the template service
            var inputVariables = new Dictionary<string, object>
            {
                ["$Source"] = entity.DefinitionId == null ? entity.Collection : $"{entity.Collection}/{entity.DefinitionId}",
                ["$Filter"] = args.Filter,
                ["$OrderBy"] = args.OrderBy,
                ["$Top"] = args.Top,
                ["$Skip"] = args.Skip
            };
            var preloadedQuery = new QueryByFilterInfo(entity.Collection, entity.DefinitionId, args.Filter, args.OrderBy, args.Top, args.Skip, ids: args.I);
            var templates = new string[] { entity.DownloadName, entity.Body };
            var culture = TemplateUtil.GetCulture(args, await _repo.GetTenantInfoAsync(cancellation));

            // Generate the output
            var outputs = await _templateService.GenerateMarkup(templates, inputVariables, preloadedQuery, culture, cancellation);
            var downloadName = AppendExtension(outputs[0], entity);
            var body = outputs[1];

            // Return as a file
            return (body, downloadName);
        }
        
        public async Task<(string Body, string DownloadName)> PreviewById(string id, MarkupPreviewTemplate entity, GenerateMarkupByIdArguments args, CancellationToken cancellation)
        {
            // Everything to input in the template service
            var inputVariables = new Dictionary<string, object>
            {
                ["$Source"] = entity.DefinitionId == null ? entity.Collection : $"{entity.Collection}/{entity.DefinitionId}",
                ["$Id"] = id ?? throw new BadRequestException("The id argument is required")
            };
            var preloadedQuery = new QueryByIdInfo(entity.Collection, entity.DefinitionId, id);
            var templates = new string[] { entity.DownloadName, entity.Body };
            var culture = TemplateUtil.GetCulture(args, await _repo.GetTenantInfoAsync(cancellation));

            // Generate the output
            var outputs = await _templateService.GenerateMarkup(templates, inputVariables, preloadedQuery, culture, cancellation);
            var downloadName = AppendExtension(outputs[0], entity);
            var body = outputs[1];

            // Return as a file
            return (body, downloadName);
        }

        private string AppendExtension(string downloadName, MarkupPreviewTemplate entity)
        {
            // Append the file extension if missing
            if (string.IsNullOrWhiteSpace(downloadName))
            {
                downloadName = _localizer["File"];
            }

            var expectedExtension = "." + entity.MarkupLanguage switch { MimeTypes.Html => "html", _ => null };
            if (expectedExtension != null && !downloadName.EndsWith(expectedExtension))
            {
                downloadName += expectedExtension;
            }

            return downloadName;
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return await _repo.UserPermissions(action, View, cancellation);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<MarkupTemplate> Search(Query<MarkupTemplate> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(MarkupTemplate.Name);
                var name2 = nameof(MarkupTemplate.Name2);
                var name3 = nameof(MarkupTemplate.Name3);
                var code = nameof(MarkupTemplate.Code);
                var desc = nameof(MarkupTemplate.Description);
                var desc2 = nameof(MarkupTemplate.Description2);
                var desc3 = nameof(MarkupTemplate.Description3);

                var filterString = $"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}' or {desc} {Ops.contains} '{search}' or {desc2} {Ops.contains} '{search}' or {desc3} {Ops.contains} '{search}'";
                query = query.Filter(filterString);
            }

            return query;
        }

        protected override async Task<List<MarkupTemplateForSave>> SavePreprocessAsync(List<MarkupTemplateForSave> entities)
        {
            var tenantInfo = await _repo.GetTenantInfoAsync(cancellation: default);
            // Defaults
            entities.ForEach(entity =>
            {
                // Set defaults
                entity.SupportsPrimaryLanguage ??= false;
                entity.SupportsSecondaryLanguage ??= false;
                entity.SupportsTernaryLanguage ??= false;

                // Make sure we adhere to company languages
                if (tenantInfo.SecondaryLanguageId == null)
                {
                    entity.SupportsSecondaryLanguage = false;
                }

                if (tenantInfo.TernaryLanguageId == null)
                {
                    entity.SupportsTernaryLanguage = false;
                }

                // Make sure at least primary language is true
                if (!entity.SupportsSecondaryLanguage.Value && !entity.SupportsTernaryLanguage.Value)
                {
                    entity.SupportsPrimaryLanguage = true;
                }

                // Collection and DefinitionId only make sense when the usage is specified
                if (entity.Usage == null)
                {
                    entity.Collection = null;
                    entity.DefinitionId = null;
                }
            });

            // SQL Preprocessing
            // await _repo.MarkupTemplates__Preprocess(entities);

            return entities;
        }

        protected override async Task SaveValidateAsync(List<MarkupTemplateForSave> entities)
        {
            foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
            {
                if (entity.Usage == MarkupTemplateConst.QueryByFilter || entity.Usage == MarkupTemplateConst.QueryById)
                {
                    if (entity.Collection == null)
                    {
                        ModelState.AddModelError($"[{index}].Collection", _localizer[Constants.Error_Field0IsRequired, _localizer["MarkupTemplate_Collection"]]);
                    }

                    if (entity.Usage == MarkupTemplateConst.QueryById)
                    {
                        // DefinitionId is required when querying by Id
                        if (entity.DefinitionId == null)
                        {
                            ModelState.AddModelError($"[{index}].DefinitionId", _localizer[Constants.Error_Field0IsRequired, _localizer["MarkupTemplate_DefinitionId"]]);
                        }
                    }
                }

                // TODO Check that DefinitionId is compatible with Collection

                if (ModelState.HasReachedMaxErrors)
                {
                    // No need to keep going forever
                    break;
                }
            }

            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                // null Ids will cause an error when calling the SQL validation
                return;
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.MarkupTemplates_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<MarkupTemplateForSave> entities, bool returnIds)
        {
            return await _repo.MarkupTemplates__Save(entities, returnIds: returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.MarkupTemplates_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.MarkupTemplates__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["MarkupTemplate"]]);
            }
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse(nameof(MarkupTemplate.Name));
        }
    }
}
