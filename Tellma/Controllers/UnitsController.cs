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

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class UnitsController : CrudControllerBase<UnitForSave, Unit, int>
    {
        public const string BASE_ADDRESS = "units";

        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;

        private string View => BASE_ADDRESS;

        public UnitsController(
            ILogger<UnitsController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
            _repo = repo;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Unit>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(() => ActivateImpl(ids: ids, args, isActive: true), _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<Unit>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(() => ActivateImpl(ids: ids, args, isActive: false), _logger);
        }

        private async Task<ActionResult<EntitiesResponse<Unit>>> ActivateImpl(List<int> ids, ActionArguments args, bool isActive)
        {
            // Parse parameters

            // Check user permissions
            await CheckActionPermissions("IsActive", ids);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.Units__Activate(ids, isActive);

            if (args.ReturnEntities ?? false)
            {
                var response = await LoadDataByIdsAndTransform(ids, args);

                trx.Complete();
                return Ok(response);
            }
            else
            {
                trx.Complete();
                return Ok();
            }
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return await _repo.UserPermissions(action, View, cancellation);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<Unit> Search(Query<Unit> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Unit.Name);
                var name2 = nameof(Unit.Name2);
                var name3 = nameof(Unit.Name3);
                var code = nameof(Unit.Code);
                var desc = nameof(Unit.Description);
                var desc2 = nameof(Unit.Description2);
                var desc3 = nameof(Unit.Description3);

                var filterString = $"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}' or {desc} {Ops.contains} '{search}' or {desc2} {Ops.contains} '{search}' or {desc3} {Ops.contains} '{search}'";
                query = query.Filter(FilterExpression.Parse(filterString));
            }

            return query;
        }

        protected override async Task SaveValidateAsync(List<UnitForSave> entities)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Units_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<UnitForSave> entities, bool returnIds)
        {
            return await _repo.Units__Save(entities, returnIds: returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Units_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.Units__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["Unit"]]);
            }
        }

        protected override Query<Unit> GetAsQuery(List<UnitForSave> entities)
        {
            throw new System.NotImplementedException();
        }
    }
}
