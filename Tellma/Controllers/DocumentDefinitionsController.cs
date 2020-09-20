using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using Tellma.Services.Utilities;
using System.Linq;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class DocumentDefinitionsController : CrudControllerBase<DocumentDefinitionForSave, DocumentDefinition, int>
    {
        public const string BASE_ADDRESS = "document-definitions";

        private readonly DocumentDefinitionsService _service;

        public DocumentDefinitionsController(DocumentDefinitionsService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpPut("update-state")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Close([FromBody] List<int> ids, [FromQuery] UpdateStateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.UpdateState(ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

                Response.Headers.Set("x-definitions-version", Constants.Stale);
                if (args.ReturnEntities ?? false)
                {
                    return Ok(response);
                }
                else
                {
                    return Ok();
                }
            },
            _logger);
        }

        protected override CrudServiceBase<DocumentDefinitionForSave, DocumentDefinition, int> GetCrudService()
        {
            return _service;
        }

        protected override Task OnSuccessfulSave(List<DocumentDefinition> data, Extras extras)
        {
            Response.Headers.Set("x-definitions-version", Constants.Stale);
            return base.OnSuccessfulSave(data, extras);
        }

        protected override Task OnSuccessfulDelete(List<int> ids)
        {
            Response.Headers.Set("x-definitions-version", Constants.Stale);
            return base.OnSuccessfulDelete(ids);
        }
    }

    public class DocumentDefinitionsService : CrudServiceBase<DocumentDefinitionForSave, DocumentDefinition, int>
    {
        private readonly ApplicationRepository _repo;
        private readonly IDefinitionsCache _defCache;

        private string View => DocumentDefinitionsController.BASE_ADDRESS;

        public DocumentDefinitionsService(ApplicationRepository repo, IDefinitionsCache defCache, IServiceProvider sp) : base(sp)
        {
            _repo = repo;
            _defCache = defCache;
        }

        public async Task<(List<DocumentDefinition>, Extras)> UpdateState(List<int> ids, UpdateStateArguments args)
        {
            // Make sure 
            int jvDefId = _defCache.GetCurrentDefinitionsIfCached()?.Data?.ManualJournalVouchersDefinitionId ??
                throw new BadRequestException("The Manual Journal Voucher Id is not defined");

            int index = 0;
            ids.ForEach(id =>
            {
                if (id == jvDefId)
                {
                    string path = $"[{index}]";
                    string msg = _localizer["Error_CannotModifySystemItem"];

                    ModelState.AddModelError(path, msg);
                }

                index++;
            });

            // No point carrying on
            ModelState.ThrowIfInvalid();

            // Check user permissions
            var action = "State";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // C# Validation 
            if (string.IsNullOrWhiteSpace(args.State))
            {
                throw new BadRequestException(_localizer[Constants.Error_Field0IsRequired, _localizer["State"]]);
            }

            if (!DefStates.All.Contains(args.State))
            {
                string validStates = string.Join(", ", DefStates.All);
                throw new BadRequestException($"'{args.State}' is not a valid definition state, valid states are: {validStates}");
            }

            // Transaction
            using var trx = ControllerUtilities.CreateTransaction();

            // Validate
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var errors = await _repo.DocumentDefinitions_Validate__UpdateState(ids, args.State, top: remainingErrorCount);
            ControllerUtilities.AddLocalizedErrors(ModelState, errors, _localizer);
            ModelState.ThrowIfInvalid();

            // Execute
            await _repo.DocumentDefinitions__UpdateState(ids, args.State);

            // Prepare response
            List<DocumentDefinition> data = null;
            Extras extras = null;

            if (args.ReturnEntities ?? false)
            {
                (data, extras) = await GetByIds(ids, args, action, cancellation: default);
            }

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, data);

            // Commit and return
            trx.Complete();
            return (data, extras);
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _repo.PermissionsFromCache(View, action, cancellation);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<DocumentDefinition> Search(Query<DocumentDefinition> query, GetArguments args)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var titleP = nameof(DocumentDefinition.TitlePlural);
                var titleP2 = nameof(DocumentDefinition.TitlePlural2);
                var titleP3 = nameof(DocumentDefinition.TitlePlural3);

                var titleS = nameof(DocumentDefinition.TitleSingular);
                var titleS2 = nameof(DocumentDefinition.TitleSingular2);
                var titleS3 = nameof(DocumentDefinition.TitleSingular3);
                var code = nameof(DocumentDefinition.Code);

                query = query.Filter($"{titleS} {Ops.contains} '{search}' or {titleS2} {Ops.contains} '{search}' or {titleS3} {Ops.contains} '{search}' or {titleP} {Ops.contains} '{search}' or {titleP2} {Ops.contains} '{search}' or {titleP3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'");
            }

            return query;
        }

        protected override Task<List<DocumentDefinitionForSave>> SavePreprocessAsync(List<DocumentDefinitionForSave> entities)
        {
            // Defaults
            entities?.ForEach(e =>
            {
                e.IsOriginalDocument ??= true;
                e.CodeWidth ??= 4;
            });

            return base.SavePreprocessAsync(entities);
        }

        protected override async Task SaveValidateAsync(List<DocumentDefinitionForSave> entities)
        {
            int docDefIndex = 0;
            entities?.ForEach(docDef =>
            {
                if (docDef.LineDefinitions == null || docDef.LineDefinitions.Count == 0)
                {
                    string path = $"[{docDefIndex}].{nameof(DocumentDefinition.LineDefinitions)}";
                    string msg = _localizer["Error_OneLineDefinitionIsRquired"];

                    ModelState.AddModelError(path, msg);
                }
                else
                {
                    // Line Definitions that are duplicated within the same document
                    var duplicateIndices = docDef.LineDefinitions
                        .Select((entity, index) => (entity.LineDefinitionId, index))
                        .GroupBy(pair => pair.LineDefinitionId)
                        .Where(g => g.Count() > 1)
                        .SelectMany(g => g)
                        .Select((_, index) => index);

                    foreach (var index in duplicateIndices)
                    {
                        string path = $"[{docDefIndex}].{nameof(DocumentDefinition.LineDefinitions)}[{index}].{nameof(DocumentDefinitionLineDefinition.LineDefinitionId)}";
                        string msg = _localizer["Error_DuplicateLineDefinition"];

                        ModelState.AddModelError(path, msg);
                    }
                }

                docDefIndex++;
            });

            // No point carrying on if invalid
            ModelState.ThrowIfInvalid();

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.DocumentDefinitions_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<DocumentDefinitionForSave> entities, bool returnIds)
        {
            return await _repo.DocumentDefinitions__Save(entities, returnIds: returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            int jvDefId = _defCache.GetCurrentDefinitionsIfCached()?.Data?.ManualJournalVouchersDefinitionId ??
                throw new BadRequestException("The Manual Journal Voucher Id is not defined");

            int index = 0;
            ids.ForEach(id =>
            {
                if (id == jvDefId)
                {
                    string path = $"[{index}]";
                    string msg = _localizer["Error_CannotModifySystemItem"];

                    ModelState.AddModelError(path, msg);
                }

                index++;
            });

            // No point carrying on
            ModelState.ThrowIfInvalid();

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.DocumentDefinitions_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.DocumentDefinitions__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["DocumentDefinition"]]);
            }
        }
    }
}
