using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/ifrs-concepts")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class IfrsConceptsController : FactGetByIdControllerBase<IfrsConcept, int>
    {
        private readonly IfrsConceptsService _service;

        public IfrsConceptsController(IfrsConceptsService service)
        {
            _service = service;
        }

        protected override FactGetByIdServiceBase<IfrsConcept, int> GetFactGetByIdService()
        {
            return _service;
        }
    }
}
