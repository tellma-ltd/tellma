using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/details-entries")]
    [ApplicationController]
    public class DetailsEntriesController : FactWithIdControllerBase<DetailsEntry, int>
    {
        private readonly DetailsEntriesService _service;

        public DetailsEntriesController(DetailsEntriesService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpGet("statement")]
        public async Task<ActionResult<StatementResponse>> GetStatement([FromQuery] StatementArguments args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (
                    data, 
                    opening, 
                    openingQuantity, 
                    openingMonetaryValue, 
                    closing, 
                    closingQuantity, 
                    closingMonetaryValue, 
                    count
                    ) = await _service.GetStatement(args, cancellation);

                // Flatten and Trim
                var relatedEntities = FlattenAndTrim(data, cancellation);

                var response = new StatementResponse
                {
                    Closing = closing,
                    ClosingQuantity = closingQuantity,
                    ClosingMonetaryValue = closingMonetaryValue,
                    Opening = opening,
                    OpeningQuantity = openingQuantity,
                    OpeningMonetaryValue = openingMonetaryValue,
                    TotalCount = count,
                    CollectionName = ControllerUtilities.GetCollectionName(typeof(DetailsEntry)),
                    RelatedEntities = relatedEntities,
                    Result = data,
                    ServerTime = serverTime,
                    Skip = args.Skip,
                    Top = data.Count
                };

                return Ok(response);
            }
            , _logger);
        }

        protected override FactWithIdServiceBase<DetailsEntry, int> GetFactWithIdService()
        {
            return _service;
        }
    }
}
