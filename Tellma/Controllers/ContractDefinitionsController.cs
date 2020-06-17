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

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class ContractDefinitionsController : CrudControllerBase<ContractDefinitionForSave, ContractDefinition, int>
    {
        public const string BASE_ADDRESS = "contract-definitions";

        private readonly ContractDefinitionsService _service;

        public ContractDefinitionsController(ILogger<ContractDefinitionsController> logger, ContractDefinitionsService service, DefinitionsService defService) : base(logger)
        {
            _service = service;
        }

        protected override CrudServiceBase<ContractDefinitionForSave, ContractDefinition, int> GetCrudService()
        {
            return _service;
        }

        protected override Task OnSuccessfulSave(List<ContractDefinition> data, Extras extras)
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

    public class ContractDefinitionsService : CrudServiceBase<ContractDefinitionForSave, ContractDefinition, int>
    {
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;

        private string View => ContractDefinitionsController.BASE_ADDRESS;

        public ContractDefinitionsService(IStringLocalizer<Strings> localizer, ApplicationRepository repo, IServiceProvider sp) : base(sp)
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

        protected override Query<ContractDefinition> Search(Query<ContractDefinition> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var titleP = nameof(ContractDefinition.TitlePlural);
                var titleP2 = nameof(ContractDefinition.TitlePlural2);
                var titleP3 = nameof(ContractDefinition.TitlePlural3);

                var titleS = nameof(ContractDefinition.TitleSingular);
                var titleS2 = nameof(ContractDefinition.TitleSingular2);
                var titleS3 = nameof(ContractDefinition.TitleSingular3);
                var code = nameof(ContractDefinition.Code);

                query = query.Filter($"{titleS} {Ops.contains} '{search}' or {titleS2} {Ops.contains} '{search}' or {titleS3} {Ops.contains} '{search}' or {titleP} {Ops.contains} '{search}' or {titleP2} {Ops.contains} '{search}' or {titleP3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'");
            }

            return query;
        }

        protected override async Task SaveValidateAsync(List<ContractDefinitionForSave> entities)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.ContractDefinitions_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<ContractDefinitionForSave> entities, bool returnIds)
        {
            return await _repo.ContractDefinitions__Save(entities, returnIds: returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.ContractDefinitions_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.ContractDefinitions__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["ContractDefinition"]]);
            }
        }
    }
}
