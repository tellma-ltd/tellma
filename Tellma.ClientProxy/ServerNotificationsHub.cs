using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Data;
using Tellma.Services;
using Tellma.Services.ApiAuthentication;
using Tellma.Services.MultiTenancy;

namespace Tellma.Controllers
{
    [AuthorizeJwtBearer]
    public class ServerNotificationsHub : Hub<INotifiedClient>
    {
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
