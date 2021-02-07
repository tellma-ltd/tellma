using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
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
    public class LookupDefinitionsController : CrudControllerBase<LookupDefinitionForSave, LookupDefinition, int>
    {
        public const string BASE_ADDRESS = "lookup-definitions";

        private readonly LookupDefinitionsService _service;

        public LookupDefinitionsController(LookupDefinitionsService service, IServiceProvider sp) : base(sp)
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

        protected override CrudServiceBase<LookupDefinitionForSave, LookupDefinition, int> GetCrudService()
        {
            return _service;
        }

        protected override Task OnSuccessfulSave(List<LookupDefinition> data, Extras extras)
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

    public class LookupDefinitionsService : CrudServiceBase<LookupDefinitionForSave, LookupDefinition, int>
    {
        private readonly ApplicationRepository _repo;

        private string View => LookupDefinitionsController.BASE_ADDRESS;

        public LookupDefinitionsService(ApplicationRepository repo, IServiceProvider sp) : base(sp)
        {
            _repo = repo;
        }

        public async Task<(List<LookupDefinition>, Extras)> UpdateState(List<int> ids, UpdateStateArguments args)
        {
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
            var errors = await _repo.LookupDefinitions_Validate__UpdateState(ids, args.State, top: remainingErrorCount);
            ControllerUtilities.AddLocalizedErrors(ModelState, errors, _localizer);
            ModelState.ThrowIfInvalid();

            // Execute
            await _repo.LookupDefinitions__UpdateState(ids, args.State);

            // Prepare response
            List<LookupDefinition> data = null;
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

        protected override Query<LookupDefinition> Search(Query<LookupDefinition> query, GetArguments args)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var titleP = nameof(LookupDefinition.TitlePlural);
                var titleP2 = nameof(LookupDefinition.TitlePlural2);
                var titleP3 = nameof(LookupDefinition.TitlePlural3);

                var titleS = nameof(LookupDefinition.TitleSingular);
                var titleS2 = nameof(LookupDefinition.TitleSingular2);
                var titleS3 = nameof(LookupDefinition.TitleSingular3);
                var code = nameof(LookupDefinition.Code);

                query = query.Filter($"{titleS} contains '{search}' or {titleS2} contains '{search}' or {titleS3} contains '{search}' or {titleP} contains '{search}' or {titleP2} contains '{search}' or {titleP3} contains '{search}' or {code} contains '{search}'");
            }

            return query;
        }

        protected override async Task SaveValidateAsync(List<LookupDefinitionForSave> entities)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.LookupDefinitions_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<LookupDefinitionForSave> entities, bool returnIds)
        {
            return await _repo.LookupDefinitions__Save(entities, returnIds: returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.LookupDefinitions_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.LookupDefinitions__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["LookupDefinition"]]);
            }
        }
    }
}
