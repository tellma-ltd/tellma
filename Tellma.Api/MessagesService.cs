using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class MessagesService : FactGetByIdServiceBase<MessageForQuery, int>
    {
        private static readonly PhoneAttribute phoneAtt = new();
        private readonly ApplicationFactServiceBehavior _behavior;

        public MessagesService(ApplicationFactServiceBehavior behavior, FactServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
        }

        protected override string View => "messages";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override Task<EntityQuery<MessageForQuery>> Search(EntityQuery<MessageForQuery> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                // Prepare the filter string
                var message = nameof(MessageForQuery.Content);
                var filterString = $"{message} contains '{search}'";

                // If the search term looks like a phone number, include the contact mobile in the search
                if (phoneAtt.IsValid(search))
                {
                    var e164 = BaseUtil.ToE164(search);
                    var toPhone = nameof(MessageForQuery.PhoneNumber);

                    filterString += $" or {toPhone} startsw '{e164}'";
                }

                // Apply the filter
                query = query.Filter(filterString);
            }

            return Task.FromResult(query);
        }
    }
}
