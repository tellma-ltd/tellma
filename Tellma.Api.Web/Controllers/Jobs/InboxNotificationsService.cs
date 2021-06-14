using System;
using System.Collections.Generic;
using System.Linq;
using Tellma.Controllers.Dto;
using Tellma.Data;

namespace Tellma.Controllers.Jobs
{
    /// <summary>
    /// This service handles internal inbox notifications through SignalR
    /// </summary>
    public class InboxNotificationsService
    {
        private readonly InboxNotificationsQueue _inboxQueue;

        public InboxNotificationsService(InboxNotificationsQueue inboxQueue)
        {
            _inboxQueue = inboxQueue;
        }

        /// <summary>
        /// Simply adds the notification to the <see cref="InboxNotificationsQueue"/>
        /// </summary>
        public void NotifyInbox(int tenantId, IEnumerable<InboxNotificationInfo> notifications, bool updateInboxList = true)
        {
            if (notifications == null || !notifications.Any())
            {
                return;
            }

            DateTimeOffset now = DateTimeOffset.Now;
            _inboxQueue.QueueBackgroundWorkItem(notifications.Select(e => FromEntity(e, tenantId, updateInboxList, now)));
        }

        #region Helper Functions

        /// <summary>
        /// Helper function
        /// </summary>
        public static InboxNotification FromEntity(InboxNotificationInfo e, int tenantId, bool updateInboxList, DateTimeOffset? serverTime)
        {
            return new InboxNotification
            {
                Count = e.Count,
                UnknownCount = e.UnknownCount,
                UpdateInboxList = updateInboxList,
                TenantId = tenantId,
                ServerTime = serverTime ?? DateTimeOffset.Now,
                ExternalId = e.ExternalId
            };
        }

        #endregion
    }
}
