using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class EmailsService : FactGetByIdServiceBase<EmailForQuery, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;

        public EmailsService(ApplicationFactServiceBehavior behavior, FactServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
        }

        protected override string View => "emails";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override Task<EntityQuery<EmailForQuery>> Search(EntityQuery<EmailForQuery> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var toEmail = nameof(EmailForQuery.ToEmail);
                var subject = nameof(EmailForQuery.Subject);

                // Prepare the filter string
                var filterString = $"{toEmail} contains '{search}' or {subject} contains '{search}'";

                // Apply the filter
                query = query.Filter(ExpressionFilter.Parse(filterString));
            }

            return Task.FromResult(query);
        }
    }
}
