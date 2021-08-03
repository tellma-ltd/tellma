using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Services.ClientProxy
{
    /// <summary>
    /// A background job that dequeues items from <see cref="InboxNotificationsQueue"/> and sends them to connected clients over SignalR.
    /// </summary>
    public class InboxNotificationsJob : BackgroundService
    {
        private readonly InboxNotificationsQueue _queue;
        private readonly IHubContext<ServerNotificationsHub, INotifiedClient> _hubContext;
        private readonly ILogger<InboxNotificationsJob> _logger;

        public InboxNotificationsJob(InboxNotificationsQueue queue, IHubContext<ServerNotificationsHub, INotifiedClient> hubContext, ILogger<InboxNotificationsJob> logger)
        {
            _queue = queue;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            _logger.LogInformation(GetType().Name + " Started.");

            while (!cancellation.IsCancellationRequested)
            {
                try // To make sure the background service keeps running
                {
                    // When the queue is empty, this goes to sleep until an item is enqueued
                    var (notifications, _) = await _queue.DequeueAsync(cancellation);

                    if (notifications == null || !notifications.Any())
                    {
                        continue;
                    }

                    // Send the notifications in parallel
                    await Task.WhenAll(notifications.Select(async (notification) =>
                    {
                        try
                        {
                            await _hubContext.Clients.User(notification.ExternalId).UpdateInbox(notification);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"Error sending SignalR notification. TenantId = {notification.TenantId}, ExternalId = {notification.ExternalId}");
                        }
                    }));
                }
                catch (TaskCanceledException) { }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in {GetType().Name}.");
                }
            }
        }
    }
}
