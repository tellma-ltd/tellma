using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/exchange-rates")]
    [ApplicationController]
    public class ExchangeRatesController : CrudControllerBase<ExchangeRateForSave, ExchangeRate, int>
    {
        private readonly ExchangeRatesService _service;

        public ExchangeRatesController(ExchangeRatesService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpGet("convert-to-functional")]
        public async Task<ActionResult<decimal>> ConvertToFunctional(DateTime date, string currencyId, decimal amount, CancellationToken cancellation)
        {
            var result = await _service.ConvertToFunctional(date, currencyId, amount, cancellation);
            return Ok(result);
        }

        protected override CrudServiceBase<ExchangeRateForSave, ExchangeRate, int> GetCrudService()
        {
            return _service;
        }
    }
}
