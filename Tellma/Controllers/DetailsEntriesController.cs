using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class DetailsEntriesController : FactWithIdControllerBase<DetailsEntry, int>
    {
        public const string BASE_ADDRESS = "details-entries";

        private readonly DetailsEntriesService _service;
        private readonly ILogger<DetailsEntriesController> _logger;

        public DetailsEntriesController(DetailsEntriesService service, ILogger<DetailsEntriesController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("statement")]
        public async Task<ActionResult<StatementResponse>> GetStatement([FromQuery] StatementArguments args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, opening, closing, count) = await _service.GetStatement(args, cancellation);

                // Flatten and Trim
                var relatedEntities = FlattenAndTrim(data, cancellation);

                var response = new StatementResponse
                {
                    Closing = closing,
                    Opening = opening,
                    TotalCount = count,
                    CollectionName = GetCollectionName(typeof(DetailsEntry)),
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

    public class DetailsEntriesService : FactWithIdServiceBase<DetailsEntry, int>
    {
        private readonly ApplicationRepository _repo;
        private readonly IHttpContextAccessor _contextAccessor;

        private string View => DetailsEntriesController.BASE_ADDRESS;

        //#region Custom Params

        //private DateTime? _openingDateOverride;
        //private DateTime? _closingDateOverride;

        //private DateTime? OpeningDate
        //{
        //    get
        //    {
        //        if (_openingDateOverride != null)
        //        {
        //            return _openingDateOverride;
        //        }
        //        else
        //        {
        //            string openingDateString = GetQueryParameter("opening");
        //            if (!string.IsNullOrWhiteSpace(openingDateString))
        //            {
        //                if (DateTime.TryParse(openingDateString, out DateTime result))
        //                {
        //                    return result;
        //                }
        //            }
        //        }

        //        return null;
        //    }
        //}

        //private DateTime? ClosingDate
        //{
        //    get
        //    {
        //        if (_closingDateOverride != null)
        //        {
        //            return _closingDateOverride;
        //        }
        //        else
        //        {
        //            string closingDateString = GetQueryParameter("closing");
        //            if (!string.IsNullOrWhiteSpace(closingDateString))
        //            {
        //                if (DateTime.TryParse(closingDateString, out DateTime result))
        //                {
        //                    return result;
        //                }
        //            }
        //        }

        //        return null;
        //    }
        //}

        //private string GetQueryParameter(string name)
        //{
        //    var query = _contextAccessor.HttpContext?.Request?.Query;
        //    if (query != null && query.TryGetValue(name, out StringValues value))
        //    {
        //        return value.FirstOrDefault();
        //    }

        //    return null;
        //}

        //public DetailsEntriesService SetIncludeOpening(DateTime val)
        //{
        //    _openingDateOverride = val;
        //    return this;
        //}

        //public DetailsEntriesService SetIncludeClosing(DateTime val)
        //{
        //    _closingDateOverride = val;
        //    return this;
        //}

        //#endregion

        public DetailsEntriesService(IServiceProvider sp, ApplicationRepository repo, IHttpContextAccessor contextAccessor) : base(sp)
        {
            _repo = repo;
            _contextAccessor = contextAccessor;
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return await _repo.UserPermissions(action, View, cancellation);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<DetailsEntry> Search(Query<DetailsEntry> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            return query;
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse(nameof(DetailsEntry.AccountId));
        }

        //public override Task<(List<DetailsEntry> Data, Extras Extras, bool IsPartial, int? Count)> GetFact(GetArguments args, CancellationToken cancellation)
        //{
        //    _filter = args.Filter;
        //    return base.GetFact(args, cancellation);
        //}

        //private string _filter;

        //protected override async Task<Extras> GetExtras(IEnumerable<DetailsEntry> result, CancellationToken cancellation)
        //{
        //    var extras = new Extras();

        //    if (OpeningDate != null || ClosingDate != null)
        //    {
        //        string exp = $"{nameof(Aggregations.sum)}({nameof(DetailsEntry.AlgebraicValue)})";
        //        var select = AggregateSelectExpression.Parse(exp);

        //        // Grab the opening
        //        if (OpeningDate != null)
        //        {
        //            var dateFilter = FilterExpression.Parse($"Line/PostingDate lt {OpeningDate.Value:yyyy-MM-dd}");
        //            var q = _repo.AggregateQuery<DetailsEntry>().Select(select).Filter(dateFilter);
        //            var qResult = (await q.ToListAsync(cancellation))[0];

        //            qResult.TryGetValue(exp, out object value);
        //            extras["Opening"] = value ?? 0;
        //        }

        //        // Grab the closing
        //        if (ClosingDate != null)
        //        {
        //            var filter = FilterExpression.Parse($"Line/PostingDate le {ClosingDate.Value:yyyy-MM-dd}");
        //            var q = _repo.AggregateQuery<DetailsEntry>().Select(select).Filter(filter);
        //            var qResult = (await q.ToListAsync(cancellation))[0];

        //            qResult.TryGetValue(exp, out object value);
        //            extras["Closing"] = value ?? 0;
        //        }
        //    }

        //    return extras;
        //}

        public async Task<(List<DetailsEntry> Data, decimal opening, decimal closing, int Count)> GetStatement(StatementArguments args, CancellationToken cancellation)
        {
            // Step 1: Prepare the filters
            StringBuilder undatedFilterBldr = new StringBuilder($"{nameof(DetailsEntry.Line)}/{nameof(LineForQuery.State)} {Ops.eq} {LineState.Finalized}");

            if (args.AccountId != null)
            {
                undatedFilterBldr.Append($" and {nameof(DetailsEntry.AccountId)} {Ops.eq} {args.AccountId.Value}");
            }

            if (args.SegmentId != null)
            {
                undatedFilterBldr.Append($" and {nameof(DetailsEntry.Line)}/{nameof(LineForQuery.Document)}/{nameof(Document.SegmentId)} {Ops.eq} {args.SegmentId.Value}");
            }

            if (args.ContractId != null)
            {
                undatedFilterBldr.Append($" and {nameof(DetailsEntry.ContractId)} {Ops.eq} {args.ContractId.Value}");
            }

            if (args.ResourceId != null)
            {
                undatedFilterBldr.Append($" and {nameof(DetailsEntry.ResourceId)} {Ops.eq} {args.ResourceId.Value}");
            }

            if (args.EntryTypeId != null)
            {
                undatedFilterBldr.Append($" and {nameof(DetailsEntry.EntryTypeId)} {Ops.eq} {args.EntryTypeId.Value}");
            }

            if (args.CenterId != null)
            {
                undatedFilterBldr.Append($" and {nameof(DetailsEntry.CenterId)} {Ops.eq} {args.CenterId.Value}");
            }

            if (!string.IsNullOrWhiteSpace(args.CurrencyId))
            {
                undatedFilterBldr.Append($" and {nameof(DetailsEntry.CurrencyId)} {Ops.eq} '{args.CurrencyId.Replace("'", "''")}'");
            }

            string undatedFilter = undatedFilterBldr.ToString();

            var beforeOpeningFilterBldr = new StringBuilder(undatedFilter);
            var betweenFilterBldr = new StringBuilder(undatedFilter);
            var beforeClosingFilterBldr = new StringBuilder(undatedFilter);

            if (args.FromDate != null)
            {
                beforeOpeningFilterBldr.Append($" and {nameof(DetailsEntry.Line)}/{nameof(LineForQuery.PostingDate)} {Ops.lt} {args.FromDate.Value:yyyy-MM-dd}");
                betweenFilterBldr.Append($" and {nameof(DetailsEntry.Line)}/{nameof(LineForQuery.PostingDate)} {Ops.ge} {args.FromDate.Value:yyyy-MM-dd}");
            }

            if (args.ToDate != null)
            {
                betweenFilterBldr.Append($" and {nameof(DetailsEntry.Line)}/{nameof(LineForQuery.PostingDate)} {Ops.le} {args.ToDate.Value:yyyy-MM-dd}");
                beforeClosingFilterBldr.Append($" and {nameof(DetailsEntry.Line)}/{nameof(LineForQuery.PostingDate)} {Ops.le} {args.ToDate.Value:yyyy-MM-dd}");
            }

            string beforeOpeningFilter = beforeOpeningFilterBldr.ToString();
            string betweenDatesFilter = betweenFilterBldr.ToString();
            string beforeClosingFilter = beforeClosingFilterBldr.ToString();

            // Step 2: Load the entries
            var factArgs = new GetArguments
            {

                Select = args.Select,
                Top = args.Top,
                Skip = 0, // args.Skip,
                OrderBy = $"{nameof(DetailsEntry.Line)}/{nameof(LineForQuery.PostingDate)},{nameof(DetailsEntry.Id)}",
                CountEntities = true,
                Filter = betweenDatesFilter,
            };

            var (data, _, _, count) = await GetFact(factArgs, cancellation);


            // Step 3: Load the opening, and closing balances
            string exp = $"{nameof(Aggregations.sum)}({nameof(DetailsEntry.AlgebraicValue)})";
            var openingArgs = new GetAggregateArguments
            {
                Filter = beforeOpeningFilter,
                Select = exp
            };

            var (openingData, _) = await GetAggregate(openingArgs, cancellation);
            openingData[0].TryGetValue(exp, out object openingObj);
            decimal opening = (decimal)(openingObj ?? 0m);

            var closingArgs = new GetAggregateArguments
            {
                Filter = beforeClosingFilter,
                Select = exp
            };

            var (closingData, _) = await GetAggregate(closingArgs, cancellation);
            closingData[0].TryGetValue(exp, out object closingObj);
            decimal closing = (decimal)(closingObj ?? 0m);


            // Add the Acc. column
            decimal acc = opening;
            foreach (var entry in data)
            {
                acc += (entry.Value ?? 0m) * entry.Direction ?? throw new InvalidOperationException("Bug: Missing Direction");
                entry.Accumulation = acc;
                entry.EntityMetadata[nameof(entry.Accumulation)] = FieldMetadata.Loaded;
            }

            data = data.Skip(args.Skip).ToList(); // Skip in memory
            return (data, opening, closing, count.Value);
        }
    }
}
