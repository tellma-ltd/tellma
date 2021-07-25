using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;

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

        public async Task<NotificationSummary> Recap(CancellationToken cancellation)
        {
            await Initialize(cancellation);

            var serverTime = DateTimeOffset.UtcNow;
            var userIdSingleton = new List<int> { UserId };
            var infos = await _behavior.Repository.InboxCounts__Load(userIdSingleton, cancellation);
            var info = infos.FirstOrDefault();

            return new NotificationSummary
            {
                Inbox = new InboxStatusToSend
                {
                    Count = info?.Count ?? 0,
                    UnknownCount = info?.UnknownCount ?? 0,
                    UpdateInboxList = true,
                    ServerTime = serverTime,
                    TenantId = _behavior.TenantId,
                },
            };
        }
    }
}
