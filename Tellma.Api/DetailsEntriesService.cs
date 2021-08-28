using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Model.Common;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class DetailsEntriesService : FactWithIdServiceBase<DetailsEntry, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;

        public DetailsEntriesService(ApplicationFactServiceBehavior behavior, FactServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
        }

        protected override string View => "details-entries";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override Task<EntityQuery<DetailsEntry>> Search(EntityQuery<DetailsEntry> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var line = nameof(DetailsEntry.Line);
                var memo = nameof(LineForQuery.Memo);
                var text1 = nameof(LineForQuery.Text1);

                var filterString = $"{line}.{memo} contains '{search}' or {line}.{text1} contains '{search}' ";
                query = query.Filter(ExpressionFilter.Parse(filterString));
            }

            return Task.FromResult(query);
        }

        protected override Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken _)
        {
            var result = ExpressionOrderBy.Parse(nameof(DetailsEntry.AccountId));
            return Task.FromResult(result);
        }

        private static string UndatedFilter(StatementArguments args)
        {
            // State == Posted
            string stateFilter = $"{nameof(DetailsEntry.Line)}.{nameof(LineForQuery.State)} eq {LineState.Posted}";
            if (args.IncludeCompleted ?? false)
            {
                // OR State == Completed
                stateFilter = $"({stateFilter} or {nameof(DetailsEntry.Line)}.{nameof(LineForQuery.State)} eq {LineState.Completed})";
            }

            var undatedFilterBldr = new StringBuilder(stateFilter);

            if (args.AccountId != null)
            {
                undatedFilterBldr.Append($" and {nameof(DetailsEntry.AccountId)} eq {args.AccountId.Value}");
            }

            if (args.AgentId != null)
            {
                undatedFilterBldr.Append($" and {nameof(DetailsEntry.AgentId)} eq {args.AgentId.Value}");
            }

            if (args.ResourceId != null)
            {
                undatedFilterBldr.Append($" and {nameof(DetailsEntry.ResourceId)} eq {args.ResourceId.Value}");
            }

            if (args.NotedAgentId != null)
            {
                undatedFilterBldr.Append($" and {nameof(DetailsEntry.NotedAgentId)} eq {args.NotedAgentId.Value}");
            }

            if (args.EntryTypeId != null)
            {
                undatedFilterBldr.Append($" and {nameof(DetailsEntry.EntryTypeId)} eq {args.EntryTypeId.Value}");
            }

            if (args.CenterId != null)
            {
                undatedFilterBldr.Append($" and {nameof(DetailsEntry.CenterId)} eq {args.CenterId.Value}");
            }

            if (!string.IsNullOrWhiteSpace(args.CurrencyId))
            {
                undatedFilterBldr.Append($" and {nameof(DetailsEntry.CurrencyId)} eq '{args.CurrencyId.Replace("'", "''")}'");
            }

            return undatedFilterBldr.ToString();
        }

        public async Task<StatementResult> GetStatement(StatementArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // Step 1: Prepare the filters
            string undatedFilter = UndatedFilter(args);

            var beforeOpeningFilterBldr = new StringBuilder(undatedFilter);
            var betweenFilterBldr = new StringBuilder(undatedFilter);
            var beforeClosingFilterBldr = new StringBuilder(undatedFilter);

            if (args.FromDate != null)
            {
                beforeOpeningFilterBldr.Append($" and {nameof(DetailsEntry.Line)}.{nameof(LineForQuery.PostingDate)} lt '{args.FromDate.Value:yyyy-MM-dd}'"); // <
                betweenFilterBldr.Append($" and {nameof(DetailsEntry.Line)}.{nameof(LineForQuery.PostingDate)} ge '{args.FromDate.Value:yyyy-MM-dd}'"); // >=
            }

            if (args.ToDate != null)
            {
                betweenFilterBldr.Append($" and {nameof(DetailsEntry.Line)}.{nameof(LineForQuery.PostingDate)} le '{args.ToDate.Value:yyyy-MM-dd}'"); // <=
                beforeClosingFilterBldr.Append($" and {nameof(DetailsEntry.Line)}.{nameof(LineForQuery.PostingDate)} le '{args.ToDate.Value:yyyy-MM-dd}'"); // <=
            }

            string beforeOpeningFilter = beforeOpeningFilterBldr.ToString();
            string betweenDatesFilter = betweenFilterBldr.ToString();
            string beforeClosingFilter = beforeClosingFilterBldr.ToString();

            // Step 2: Load the entries
            var factArgs = new GetArguments
            {
                Select = args.Select,
                Top = args.Skip + args.Top, // We need this to compute openining balance, we do the skipping later in memory
                Skip = 0, // args.Skip,
                OrderBy = $"{nameof(DetailsEntry.Line)}.{nameof(LineForQuery.PostingDate)},{nameof(DetailsEntry.Id)}",
                CountEntities = true,
                Filter = betweenDatesFilter,
            };

            var result = await GetEntities(factArgs, cancellation);
            var data = result.Data;
            var count = result.Count;

            // Step 3: Load the opening balances
            string valueExp = $"sum({nameof(DetailsEntry.Value)} * {nameof(DetailsEntry.Direction)})";
            string quantityExp = $"sum({nameof(DetailsEntry.BaseQuantity)} * {nameof(DetailsEntry.Direction)})";
            string monetaryValueExp = $"sum({nameof(DetailsEntry.MonetaryValue)} * {nameof(DetailsEntry.Direction)})";
            var openingArgs = new GetAggregateArguments
            {
                Filter = beforeOpeningFilter,
                Select = $"{valueExp},{quantityExp},{monetaryValueExp}"
            };

            var openingResult = await GetAggregate(openingArgs, cancellation);
            var openingData = openingResult.Data;

            decimal opening = (decimal)(openingData[0][0] ?? 0m);
            decimal openingQuantity = (decimal)(openingData[0][1] ?? 0m);
            decimal openingMonetaryValue = (decimal)(openingData[0][2] ?? 0m);

            // step (4) Add the Acc. column
            decimal acc = opening;
            decimal accQuantity = openingQuantity;
            decimal accMonetaryValue = openingMonetaryValue;
            foreach (var entry in data)
            {
                acc += (entry.Value ?? 0m) * entry.Direction ?? throw new InvalidOperationException("Bug: Missing Direction");
                entry.Accumulation = acc;
                entry.EntityMetadata[nameof(entry.Accumulation)] = FieldMetadata.Loaded;

                accQuantity += (entry.BaseQuantity ?? 0m) * entry.Direction ?? throw new InvalidOperationException("Bug: Missing Direction");
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

                var closingResult = await GetAggregate(closingArgs, cancellation);
                var closingData = closingResult.Data;

                closing = (decimal)(closingData[0][0] ?? 0m);
                closingQuantity = (decimal)(closingData[0][1] ?? 0m);
                closingMonetaryValue = (decimal)(closingData[0][2] ?? 0m);
            }
            else
            {
                closing = acc;
                closingQuantity = accQuantity;
                closingMonetaryValue = accMonetaryValue;
            }

            data = data.Skip(args.Skip).ToList(); // Skip in memory
            return new StatementResult(data, opening, openingQuantity, openingMonetaryValue, closing, closingQuantity, closingMonetaryValue, count.Value);
        }
    }
}
