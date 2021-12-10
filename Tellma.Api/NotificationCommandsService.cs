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
    public class NotificationCommandsService : FactGetByIdServiceBase<NotificationCommand, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;

        public NotificationCommandsService(ApplicationFactServiceBehavior behavior, FactServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
        }

        protected override string View => "notification-commands";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override Task<EntityQuery<NotificationCommand>> Search(EntityQuery<NotificationCommand> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                // Prepare the filter string
                var caption = nameof(NotificationCommand.Caption);
                var filterString = $"{caption} contains '{search}'";

                // Apply the filter
                query = query.Filter(filterString);
            }

            return Task.FromResult(query);
        }
    }
}
