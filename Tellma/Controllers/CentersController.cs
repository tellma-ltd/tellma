using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class CentersController : CrudTreeControllerBase<CenterForSave, Center, int>
    {
        public const string BASE_ADDRESS = "centers";

        private readonly CentersService _service;
        private readonly ILogger _logger;

        public CentersController(CentersService service, ILogger<CentersController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Center>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
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
        public async Task<ActionResult<EntitiesResponse<Center>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Deactivate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);

            }, _logger);
        }

        protected override CrudTreeServiceBase<CenterForSave, Center, int> GetCrudTreeService()
        {
            return _service;
        }
    }

    public class CentersService : CrudTreeServiceBase<CenterForSave, Center, int>
    {
        private readonly IStringLocalizer<Strings> _localizer;
        private readonly ApplicationRepository _repo;

        private string View => CentersController.BASE_ADDRESS;

        public CentersService(IStringLocalizer<Strings> localizer, ApplicationRepository repo) : base(localizer)
        {
            _localizer = localizer;
            _repo = repo;
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return await _repo.UserPermissions(action, View, cancellation);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<Center> Search(Query<Center> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Center.Name);
                var name2 = nameof(Center.Name2);
                var name3 = nameof(Center.Name3);
                var code = nameof(Center.Code);

                query = query.Filter($"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'");
            }

            return query;
        }

        protected override async Task SaveValidateAsync(List<CenterForSave> entities)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Centers_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<CenterForSave> entities, bool returnIds)
        {
            return await _repo.Centers__Save(entities, returnIds: returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Centers_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.Centers__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["Center"]]);
            }
        }

        protected override async Task ValidateDeleteWithDescendantsAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Centers_Validate__DeleteWithDescendants(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteWithDescendantsAsync(List<int> ids)
        {
            try
            {
                await _repo.Centers__DeleteWithDescendants(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["Center"]]);
            }
        }

        public Task<(List<Center>, Extras)> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<(List<Center>, Extras)> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<(List<Center>, Extras)> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            // Check user permissions
            await CheckActionPermissions("IsActive", ids);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.Centers__Activate(ids, isActive);

            if (args.ReturnEntities ?? false)
            {
                var (data, extras) = await GetByIds(ids, args, cancellation: default);

                trx.Complete();
                return (data, extras);
            }
            else
            {
                trx.Complete();
                return (null, null);
            }
        }
    }
}