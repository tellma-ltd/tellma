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
    public class ResourceDefinitionsController : CrudControllerBase<ResourceDefinitionForSave, ResourceDefinition, int>
    {
        public const string BASE_ADDRESS = "resource-definitions";

        private readonly ResourceDefinitionsService _service;

        public ResourceDefinitionsController(ILogger<ResourceDefinitionsController> logger, ResourceDefinitionsService service, DefinitionsService defService) : base(logger)
        {
            _service = service;
        }

        protected override CrudServiceBase<ResourceDefinitionForSave, ResourceDefinition, int> GetCrudService()
        {
            return _service;
        }

        protected override Task OnSuccessfulSave(List<ResourceDefinition> data, Extras extras)
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

    public class ResourceDefinitionsService : CrudServiceBase<ResourceDefinitionForSave, ResourceDefinition, int>
    {
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;

        private string View => ResourceDefinitionsController.BASE_ADDRESS;

        public ResourceDefinitionsService(IStringLocalizer<Strings> localizer, ApplicationRepository repo, IServiceProvider sp) : base(sp)
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

        protected override Query<ResourceDefinition> Search(Query<ResourceDefinition> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var titleP = nameof(ResourceDefinition.TitlePlural);
                var titleP2 = nameof(ResourceDefinition.TitlePlural2);
                var titleP3 = nameof(ResourceDefinition.TitlePlural3);

                var titleS = nameof(ResourceDefinition.TitleSingular);
                var titleS2 = nameof(ResourceDefinition.TitleSingular2);
                var titleS3 = nameof(ResourceDefinition.TitleSingular3);
                var code = nameof(ResourceDefinition.Code);

                query = query.Filter($"{titleS} {Ops.contains} '{search}' or {titleS2} {Ops.contains} '{search}' or {titleS3} {Ops.contains} '{search}' or {titleP} {Ops.contains} '{search}' or {titleP2} {Ops.contains} '{search}' or {titleP3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'");
            }

            return query;
        }

        protected override async Task SaveValidateAsync(List<ResourceDefinitionForSave> entities)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.ResourceDefinitions_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<ResourceDefinitionForSave> entities, bool returnIds)
        {
            return await _repo.ResourceDefinitions__Save(entities, returnIds: returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.ResourceDefinitions_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.ResourceDefinitions__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["ResourceDefinition"]]);
            }
        }
    }
}
