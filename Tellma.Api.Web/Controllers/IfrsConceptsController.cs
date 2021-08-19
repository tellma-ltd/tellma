using Microsoft.AspNetCore.Mvc;
using System;
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

        public IfrsConceptsController(IfrsConceptsService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        protected override FactGetByIdServiceBase<IfrsConcept, int> GetFactGetByIdService()
        {
            return _service;
        }
    }
}
