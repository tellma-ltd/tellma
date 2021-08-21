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
    [Route("api/line-definitions")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class LineDefinitionsController : CrudControllerBase<LineDefinitionForSave, LineDefinition, int>
    {
        private readonly LineDefinitionsService _service;

        public LineDefinitionsController(LineDefinitionsService service)
        {
            _service = service;
        }

        protected override CrudServiceBase<LineDefinitionForSave, LineDefinition, int> GetCrudService()
        {
            return _service;
        }

        protected override Task OnSuccessfulSave(EntitiesResult<LineDefinition> data)
        {
            Response.Headers.Set("x-definitions-version", Constants.Stale);
            return base.OnSuccessfulSave(data);
        }

        protected override Task OnSuccessfulDelete(List<int> ids)
        {
            Response.Headers.Set("x-definitions-version", Constants.Stale);
            return base.OnSuccessfulDelete(ids);
        }
    }
}
