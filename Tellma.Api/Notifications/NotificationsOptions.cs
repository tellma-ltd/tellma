namespace Tellma.Api.Notifications
{
    public class NotificationsOptions
    {
        /// <summary>
        /// How long should the notification remain in state PENDING before it's considered
        /// stale (ie the instance that was working on it previously was shut down prematurely). <br/>
        /// - Too long => an interrupted notification may take a while to be retried. <br/>
        /// - Too short => a notification that is still being processed by a non-parent may end up also getting picked up by the parent and sent twice. <br/>
        /// - Happy medium => a safe margin above the worst case scenario of how long sending a batch of notifications could take. <br/>
        /// </summary>
        public int PendingNotificationExpiryInSeconds { get; set; } = 2 * 60; // 2 minutes

        /// <summary>
        /// How often to check every adopted tenant for new/state pending notifications. <br/>
        /// - Too often => the instance thread pool may get starved by repeated checks, until all tenants are checked. <br/>
        /// - Too rarely => New or Stale notifications may remain so for too long. <br/>
        /// - Happy medium => a generous duration that would not cause major alarm for users if notifications
        ///     were that late. Notifications in the vast majority of cases are sent by the original
        ///     instance in real time anyways and are only queued and checked by the parent instance
        ///     in the rare situation that the original instance died prematurely before sending them. <br/>
        /// </summary>
        public int NotificationCheckFrequencyInSeconds { get; set; } = 10 * 60; // 10 minutes

        /// <summary>
        /// This allows us to disable automatic notifications in development environment, since a 
        /// situation can arise where we restore an old production database in development and the
        /// automatic notifications engine will attempt to replay the entire history of missed
        /// notifications and may end up sending tens of thousands of emails and SMS. So we automatic
        /// notifications should always be turned off in development environments as a safety feature.
        /// </summary>
        public bool EnableAutomaticNotifications { get; set; } = false;
    }
}
