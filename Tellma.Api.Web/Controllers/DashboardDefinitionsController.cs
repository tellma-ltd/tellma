using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    [Route("api/dashboard-definitions")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class DashboardDefinitionsController : CrudControllerBase<DashboardDefinitionForSave, DashboardDefinition, int>
    {
        private readonly DashboardDefinitionsService _service;

        public DashboardDefinitionsController(DashboardDefinitionsService service)
        {
            _service = service;
        }

        protected override CrudServiceBase<DashboardDefinitionForSave, DashboardDefinition, int> GetCrudService()
        {
            return _service;
        }

        protected override Task OnSuccessfulSave(EntitiesResult<DashboardDefinition> result)
        {
            Response.Headers.Set("x-definitions-version", Constants.Stale);
            return base.OnSuccessfulSave(result);
        }

        protected override Task OnSuccessfulDelete(List<int> ids)
        {
            Response.Headers.Set("x-definitions-version", Constants.Stale);
            return base.OnSuccessfulDelete(ids);
        }
    }
}
