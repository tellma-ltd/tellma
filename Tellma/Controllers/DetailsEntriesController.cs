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
                var (data, opening, openingQuantity, openingMonetaryValue, closing, closingQuantity, closingMonetaryValue, count) = await _service.GetStatement(args, cancellation);

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

        //[HttpGet("custodian-accounts")]
        //public async Task<ActionResult<EntitiesResponse<Account>>> GetCustodianAccounts(CancellationToken cancellation)
        //{

        //    return await ControllerUtilities.InvokeActionImpl(async () =>
        //    {
        //        var serverTime = DateTimeOffset.UtcNow;
        //        var data = await _service.GetCustodianAccounts(cancellation);

        //        // Flatten and Trim
        //        var relatedEntities = FlattenAndTrim(data, cancellation);

        //        // Prepare result
        //        var result = new EntitiesResponse<Account>
        //        {
        //            ServerTime = serverTime,
        //            CollectionName = GetCollectionName(typeof(Account)),
        //            Result = data,
        //            RelatedEntities = relatedEntities,
        //        };

        //        // Return result
        //        return Ok(result);
        //    }
        //    , _logger);
        //}

        protected override FactWithIdServiceBase<DetailsEntry, int> GetFactWithIdService()
        {
            return _service;
        }
    }

    public class DetailsEntriesService : FactWithIdServiceBase<DetailsEntry, int>
    {
        private readonly ApplicationRepository _repo;

        private string View => DetailsEntriesController.BASE_ADDRESS;

        public DetailsEntriesService(IServiceProvider sp, ApplicationRepository repo) : base(sp)
        {
            _repo = repo;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _repo.PermissionsFromCache(View, action, cancellation);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<DetailsEntry> Search(Query<DetailsEntry> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var line = nameof(DetailsEntry.Line);
                var memo = nameof(LineForQuery.Memo);

                var filterString = $"{line}/{memo} {Ops.contains} '{search}'";
                query = query.Filter(FilterExpression.Parse(filterString));
            }

            return query;
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse(nameof(DetailsEntry.AccountId));
        }

        private string UndatedFilter(StatementArguments args)
        {
            // State == Finalized
            string stateFilter = $"{nameof(DetailsEntry.Line)}/{nameof(LineForQuery.State)} {Ops.eq} {LineState.Finalized}";
            if (!(args.IncludeCompleted ?? false))
            {
                // OR State == Completed
                stateFilter = $"({stateFilter} or {nameof(DetailsEntry.Line)}/{nameof(LineForQuery.State)} {Ops.eq} {LineState.Completed})";
            }

            StringBuilder undatedFilterBldr = new StringBuilder(stateFilter);

            if (args.AccountId != null)
            {
                undatedFilterBldr.Append($" and {nameof(DetailsEntry.AccountId)} {Ops.eq} {args.AccountId.Value}");
            }

            if (args.SegmentId != null)
            {
                undatedFilterBldr.Append($" and {nameof(DetailsEntry.Line)}/{nameof(LineForQuery.Document)}/{nameof(Document.SegmentId)} {Ops.eq} {args.SegmentId.Value}");
            }

            if (args.CustodianId != null)
            {
                undatedFilterBldr.Append($" and {nameof(DetailsEntry.CustodianId)} {Ops.eq} {args.CustodianId.Value}");
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

            return undatedFilterBldr.ToString();
        }

        public async Task<(
            List<DetailsEntry> Data,
            decimal opening,
            decimal openingQuantity,
            decimal openingMonetaryValue,
            decimal closing,
            decimal closingQuantity,
            decimal closingMonetaryValue,
            int Count
            )> GetStatement(StatementArguments args, CancellationToken cancellation)
        {
            // Step 1: Prepare the filters
            string undatedFilter = UndatedFilter(args);

            var beforeOpeningFilterBldr = new StringBuilder(undatedFilter);
            var betweenFilterBldr = new StringBuilder(undatedFilter);
            var beforeClosingFilterBldr = new StringBuilder(undatedFilter);

            if (args.FromDate != null)
            {
                beforeOpeningFilterBldr.Append($" and {nameof(DetailsEntry.Line)}/{nameof(LineForQuery.PostingDate)} {Ops.lt} {args.FromDate.Value:yyyy-MM-dd}"); // <
                betweenFilterBldr.Append($" and {nameof(DetailsEntry.Line)}/{nameof(LineForQuery.PostingDate)} {Ops.ge} {args.FromDate.Value:yyyy-MM-dd}"); // >=
            }

            if (args.ToDate != null)
            {
                betweenFilterBldr.Append($" and {nameof(DetailsEntry.Line)}/{nameof(LineForQuery.PostingDate)} {Ops.le} {args.ToDate.Value:yyyy-MM-dd}"); // <=
                beforeClosingFilterBldr.Append($" and {nameof(DetailsEntry.Line)}/{nameof(LineForQuery.PostingDate)} {Ops.le} {args.ToDate.Value:yyyy-MM-dd}"); // <=
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

            // Step 3: Load the opening balances
            string valueExp = $"{nameof(Aggregations.sum)}({nameof(DetailsEntry.AlgebraicValue)})";
            string quantityExp = $"{nameof(Aggregations.sum)}({nameof(DetailsEntry.AlgebraicQuantity)})";
            string monetaryValueExp = $"{nameof(Aggregations.sum)}({nameof(DetailsEntry.AlgebraicMonetaryValue)})";
            var openingArgs = new GetAggregateArguments
            {
                Filter = beforeOpeningFilter,
                Select = $"{valueExp},{quantityExp},{monetaryValueExp}"
            };

            var (openingData, _) = await GetAggregate(openingArgs, cancellation);
            openingData[0].TryGetValue(valueExp, out object openingObj);
            decimal opening = (decimal)(openingObj ?? 0m);

            openingData[0].TryGetValue(quantityExp, out object openingQuantityObj);
            decimal openingQuantity = (decimal)(openingQuantityObj ?? 0m);

            openingData[0].TryGetValue(monetaryValueExp, out object openingMonetaryValueObj);
            decimal openingMonetaryValue = (decimal)(openingMonetaryValueObj ?? 0m);

            // step (4) Add the Acc. column
            decimal acc = opening;
            decimal accQuantity = openingQuantity;
            decimal accMonetaryValue = openingMonetaryValue;
            foreach (var entry in data)
            {
                acc += (entry.Value ?? 0m) * entry.Direction ?? throw new InvalidOperationException("Bug: Missing Direction");
                entry.Accumulation = acc;
                entry.EntityMetadata[nameof(entry.Accumulation)] = FieldMetadata.Loaded;

                accQuantity += (entry.Quantity ?? 0m) * entry.Direction ?? throw new InvalidOperationException("Bug: Missing Direction");
                entry.QuantityAccumulation = accQuantity;
                entry.EntityMetadata[nameof(entry.QuantityAccumulation)] = FieldMetadata.Loaded;

                accMonetaryValue += (entry.MonetaryValue ?? 0m) * entry.Direction ?? throw new InvalidOperationException("Bug: Missing Direction");
                entry.MonetaryValueAccumulation = accMonetaryValue;
                entry.EntityMetadata[nameof(entry.MonetaryValueAccumulation)] = FieldMetadata.Loaded;
            }

            // Step (5) Load closing (if the data page is not the complete result)
            decimal closing;
            decimal closingQuantity;
            decimal closingMonetaryValue;
            if (args.Skip + args.Top >= count.Value)
            {
                var closingArgs = new GetAggregateArguments
                {
                    Filter = beforeClosingFilter,
                    Select = $"{valueExp},{quantityExp},{monetaryValueExp}"
                };

                var (closingData, _) = await GetAggregate(closingArgs, cancellation);
                closingData[0].TryGetValue(valueExp, out object closingObj);
                closing = (decimal)(closingObj ?? 0m);

                closingData[0].TryGetValue(quantityExp, out object closingQuantityObj);
                closingQuantity = (decimal)(closingQuantityObj ?? 0m);

                closingData[0].TryGetValue(monetaryValueExp, out object closingMonetaryValueObj);
                closingMonetaryValue = (decimal)(closingMonetaryValueObj ?? 0m);
            }
            else
            {
                closing = acc;
                closingQuantity = accQuantity;
                closingMonetaryValue = accMonetaryValue;
            }

            data = data.Skip(args.Skip).ToList(); // Skip in memory
            return (data, opening, openingQuantity, openingMonetaryValue, closing, closingQuantity, closingMonetaryValue, count.Value);
        }

        //public async Task<List<Account>> GetCustodianAccounts(int custodianId, CancellationToken cancellation)
        //{
        //    string accountIdProp = nameof(DetailsEntry.AccountId);
        //    string custodianIdProp = nameof(DetailsEntry.CustodianId);
        //    string algebraicValueSum = $"sum({nameof(DetailsEntry.AlgebraicValue)})";

        //    var (data, _) = await GetAggregate(new GetAggregateArguments
        //    {
        //        Select = $"{accountIdProp},{algebraicValueSum}",
        //        Filter = $""
        //    },
        //    cancellation);

        //    var dic = data.Select(e => ((int?)e[accountIdProp], (decimal?)e[algebraicValueSum]))

        //    var ids = data.Select(e => e[accountIdProp]).Where(e => e != null).Select(e => (int)e).ToList();
        //    var result = await GetByIds(ids, new SelectExpandArguments { Select = "$Details" }, cancellation);

        //    var dic = data.Se

        //    return result;
        //}
    }
}
