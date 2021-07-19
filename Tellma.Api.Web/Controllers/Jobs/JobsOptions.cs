namespace Tellma.Controllers.Jobs
{
    public class JobsOptions
    {
        /// <summary>
        /// How often do instances signal their livelihood in the Admin DB.
        /// Too often => the Admin DB will be overwhelmed.
        /// Too rarely => orphan tenants may remain orphans for too long before adopted by another instance.
        /// Happy medium => A frequency where 50 instances beating together are still far from overwhelming the admin database.
        /// </summary>
        public int InstanceHeartRateInSeconds { get; set; } = 60; // 1 minute

        /// <summary>
        /// How many orphans every instance adopts every <see cref="OrphanAdoptionFrequencyInSeconds"/>.
        /// Too many => will cause an adoption imbalance; one instance adopts many more tenants than other instances.
        /// Too few => Orphans may remain orphans for a while before adopted.
        /// Happy medium => expected number of tenants / expected number of instances (ballpark figure).
        /// </summary>
        public int OrphanAdoptionBatchCount { get; set; } = 20;

        /// <summary>
        /// How long should the notification remain in state PENDING before it's considered
        /// stale (ie the instance that was working on it previously was shut down prematurely)
        /// Too long => an interrupted notification may take a while to be retried
        /// Too short => a notification that is still being processed by a non-parent may end up also getting picked up by the parent and sent twice
        /// Happy medium => a safe margin above the worst case scenario of how long sending a batch of notifications could take
        /// </summary>
        public int PendingNotificationExpiryInSeconds { get; set; } = 2 * 60; // 2 minutes

        /// <summary>
        /// How often to check every adopted tenant for new/state pending notifications.
        /// Too often => the instance thread pool may get starved by repeated checks, 
        ///     each check consuming <see cref="NotificationCheckDegreeOfParallelism"/> of threads, until all tenants are checked
        /// Too rarely => New or Stale notifications may remain so for too long.
        /// Happy medium => a generous duration that would not cause major alarm for users if notifications
        ///     were that late. Notifications in the vast majority of cases are sent by the original
        ///     instance in real time anyways and are only queued and checked by the parent instance
        ///     in the rare situation that the original instance died prematurely before sending them.
        /// </summary>
        public int NotificationCheckFrequencyInSeconds { get; set; } = 10 * 60; // 10 minutes

        /// <summary>
        /// Autocomputed.
        /// </summary>
        public int OrphanAdoptionFrequencyInSeconds => InstanceHeartRateInSeconds * 5;

        /// <summary>
        /// Autocomputed.
        /// </summary>
        public int InstanceKeepAliveInSeconds => InstanceHeartRateInSeconds * 10;
    }
}
