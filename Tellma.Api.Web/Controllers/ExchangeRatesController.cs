using Tellma.Controllers.Utilities;
using Tellma.Model.Application;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Threading;
using Tellma.Api.Base;
using Tellma.Api;

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
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await _service.ConvertToFunctional(date, currencyId, amount, cancellation);
                return Ok(result);
            },
            _logger);
        }

        protected override CrudServiceBase<ExchangeRateForSave, ExchangeRate, int> GetCrudService()
        {
            return _service;
        }
    }
}
