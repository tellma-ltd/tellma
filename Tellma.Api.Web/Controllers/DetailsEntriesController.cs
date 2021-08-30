using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/details-entries")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class DetailsEntriesController : FactWithIdControllerBase<DetailsEntry, int>
    {
        private readonly DetailsEntriesService _service;

        public DetailsEntriesController(DetailsEntriesService service)
        {
            _service = service;
        }

        [HttpGet("statement")]
        public async Task<ActionResult<StatementResponse>> GetStatement([FromQuery] StatementArguments args, CancellationToken cancellation)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var result = await _service.GetStatement(args, cancellation);

            // Flatten and Trim
            var relatedEntities = Flatten(result.Data, cancellation);

            var response = new StatementResponse
            {
                Closing = result.Closing,
                ClosingQuantity = result.ClosingQuantity,
                ClosingMonetaryValue = result.ClosingMonetaryValue,
                Opening = result.Opening,
                OpeningQuantity = result.OpeningQuantity,
                OpeningMonetaryValue = result.OpeningMonetaryValue,
                TotalCount = result.Count,
                CollectionName = ControllerUtilities.GetCollectionName(typeof(DetailsEntry)),
                RelatedEntities = relatedEntities,
                Result = result.Data,
                ServerTime = serverTime,
                Skip = args.Skip,
                Top = result.Data.Count
            };

            return Ok(response);
        }

        protected override FactWithIdServiceBase<DetailsEntry, int> GetFactWithIdService()
        {
            return _service;
        }
    }
}
