using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Repository.Application;

namespace Tellma.Api
{
    public class StatusService : ServiceBase
    {
        private readonly ApplicationServiceBehavior _behavior;

        public StatusService(ApplicationServiceBehavior behavior, IServiceContextAccessor accessor) : base(accessor)
        {
            _behavior = behavior;
        }

        protected override IServiceBehavior Behavior => _behavior;

        public async Task<InboxStatus> Recap(CancellationToken cancellation)
        {
            await Initialize(cancellation);

            var userIdSingleton = new List<int> { UserId };
            var statuses = await _behavior.Repository.InboxCounts__Load(userIdSingleton, cancellation);
            var status = statuses.FirstOrDefault();

            return status;
        }
    }
}
