using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Data;
using Tellma.Services.ApiAuthentication;
using Tellma.Services.MultiTenancy;

namespace Tellma.Controllers
{
    [Route("api/notifications")]
    [AuthorizeAccess]
    [ApplicationController(allowUnobtrusive: true)]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ServerNotificationsController : ControllerBase
    {
        private readonly ApplicationRepository _repo;
        private readonly ITenantIdAccessor _tenantIdAccessor;

        public ServerNotificationsController(ApplicationRepository repo, ITenantIdAccessor tenantIdAccessor)
        {
            _repo = repo;
            _tenantIdAccessor = tenantIdAccessor;
        }

        /// <summary>
        /// When a client connects for the first time, or reconnects after going offline,
        /// it invokes this method to catch up on what it has missed
        /// </summary>
        /// <returns>A summary of what the client has missed</returns>
        [HttpGet("recap")]
        public async Task<ServerNotificationSummary> Recap(CancellationToken cancellation)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var userInfo = await _repo.GetUserInfoAsync(cancellation);

            var userIdSingleton = new List<int> { userInfo.UserId.Value };
            var info = (await _repo.InboxCounts__Load(userIdSingleton, cancellation)).FirstOrDefault();

            var tenantId = _tenantIdAccessor.GetTenantId();
            return new ServerNotificationSummary
            {
                Inbox = new InboxNotification
                {
                    Count = info?.Count ?? 0,
                    UnknownCount = info?.UnknownCount ?? 0,
                    UpdateInboxList = true,
                    ServerTime = serverTime,
                    TenantId = tenantId,
                },
                Notifications = new NotificationsNotification
                {
                    // TODO
                    Count = 0,
                    UnknownCount = 0,
                    ServerTime = serverTime,
                    TenantId = tenantId,
                },
            };
        }
    }

    [AuthorizeAccess]
    public class ServerNotificationsHub : Hub<INotifiedClient>
    {
    }

    public static class ServerNotificationsHubExtensions
    {
        public static async Task NotifyInboxAsync(this IHubContext<ServerNotificationsHub, INotifiedClient> hub, int tenantId, IEnumerable<InboxNotificationInfo> infos, bool updateInboxList = true)
        {
            if (infos == null || !infos.Any())
            {
                return;
            }

            var serverTime = DateTimeOffset.UtcNow;
            var tasks = new List<Task>();

            foreach (var info in infos)
            {
                tasks.Add(hub.Clients.User(info.ExternalId).UpdateInbox(new InboxNotification
                {
                    Count = info.Count,
                    UnknownCount = info.UnknownCount,
                    UpdateInboxList = updateInboxList,
                    ServerTime = serverTime,
                    TenantId = tenantId,
                }));
            }

            await Task.WhenAll(tasks);
        }
    }

    /// <summary>
    /// The methods here are implemented on the client side
    /// </summary>
    public interface INotifiedClient
    {
        Task UpdateInbox(InboxNotification notification);
        Task UpdateNotifications(NotificationsNotification notification);
        Task InvalidateCache(CacheNotification notification);
    }

    public static class CacheTypes
    {
        public const string Definitions = nameof(Definitions);
        public const string Settings = nameof(Settings);
        public const string UserSettings = nameof(UserSettings);
        public const string Permissions = nameof(Permissions);
    }
}
