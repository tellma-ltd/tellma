using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class IfrsConceptsService : FactGetByIdServiceBase<IfrsConcept, int>
    {

        private readonly ApplicationFactServiceBehavior _behavior;

        public IfrsConceptsService(ApplicationFactServiceBehavior behavior, FactServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
        }

        protected override string View => "ifrs-concepts";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override Task<EntityQuery<IfrsConcept>> Search(EntityQuery<IfrsConcept> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var labelProp = nameof(IfrsConcept.Label);
                var label2Prop = nameof(IfrsConcept.Label2);
                var label3Prop = nameof(IfrsConcept.Label3);

                // Prepare the filter string
                var filterString = $"{labelProp} contains '{search}' or {label2Prop} contains '{search}' or {label3Prop} contains '{search}'";

                // Apply the filter
                query = query.Filter(ExpressionFilter.Parse(filterString));
            }

            return Task.FromResult(query);
        }
    }
}
