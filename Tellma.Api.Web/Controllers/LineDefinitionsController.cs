using Microsoft.AspNetCore.Mvc;
using System;
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
    public class LineDefinitionsController : CrudControllerBase<LineDefinitionForSave, LineDefinition, int>
    {
        private readonly LineDefinitionsService _service;

        public LineDefinitionsController(LineDefinitionsService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        protected override CrudServiceBase<LineDefinitionForSave, LineDefinition, int> GetCrudService()
        {
            return _service;
        }

        protected override Task OnSuccessfulSave(List<LineDefinition> data, Extras extras)
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
}
