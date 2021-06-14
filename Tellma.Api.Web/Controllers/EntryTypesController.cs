using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Model.Application;
using Tellma.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.IdentityModel.Tokens;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class EntryTypesController : CrudTreeControllerBase<EntryTypeForSave, EntryType, int>
    {
        public const string BASE_ADDRESS = "entry-types";

        private readonly EntryTypesService _service;

        public EntryTypesController(EntryTypesService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<EntryType>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Activate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);

            }, _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<EntryType>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Deactivate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);

            }, _logger);
        }

        protected override CrudTreeServiceBase<EntryTypeForSave, EntryType, int> GetCrudTreeService()
        {
            return _service;
        }
    }

    public class EntryTypesService : CrudTreeServiceBase<EntryTypeForSave, EntryType, int>
    {
        private readonly ApplicationRepository _repo;

        private string View => EntryTypesController.BASE_ADDRESS;

        public EntryTypesService(ApplicationRepository repo, IServiceProvider sp) : base(sp)
        {
            _repo = repo;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _repo.PermissionsFromCache(View, action, cancellation);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<EntryType> Search(Query<EntryType> query, GetArguments args)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(EntryType.Name);
                var name2 = nameof(EntryType.Name2);
                var name3 = nameof(EntryType.Name3);
                var code = nameof(EntryType.Code);

                query = query.Filter($"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}' or {code} contains '{search}'");
            }

            return query;
        }

        protected override Task<List<EntryTypeForSave>> SavePreprocessAsync(List<EntryTypeForSave> entities)
        {
            entities.ForEach(entity =>
            {
                entity.IsAssignable ??= true;
            });

            return Task.FromResult(entities);
        }

        protected override async Task SaveValidateAsync(List<EntryTypeForSave> entities)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.EntryTypes_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<EntryTypeForSave> entities, bool returnIds)
        {
            return await _repo.EntryTypes__Save(entities, returnIds: returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.EntryTypes_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.EntryTypes__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["EntryType"]]);
            }
        }

        protected override async Task ValidateDeleteWithDescendantsAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.EntryTypes_Validate__DeleteWithDescendants(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteWithDescendantsAsync(List<int> ids)
        {
            try
            {
                await _repo.EntryTypes__DeleteWithDescendants(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["EntryType"]]);
            }
        }

        public Task<(List<EntryType>, Extras)> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<(List<EntryType>, Extras)> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<(List<EntryType>, Extras)> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.EntryTypes__Activate(ids, isActive);

            List<EntryType> data = null;
            Extras extras = null;

            if (args.ReturnEntities ?? false)
            {
                (data, extras) = await GetByIds(ids, args, action, cancellation: default);
            }

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, data);

            trx.Complete();
            return (data, extras);
        }
    }
}