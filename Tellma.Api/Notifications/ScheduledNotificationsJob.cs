using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Api.Instances;
using Tellma.Api.Templating;
using Tellma.Model.Application;
using Tellma.Repository.Application;
using Tellma.Utilities.Common;
using Tellma.Utilities.Sms;

namespace Tellma.Api.Notifications
{
    /// <summary>
    /// This job is responsible for observing the schedules of automatic notifications and sending them accordingly.
    /// </summary>
    public class ScheduledNotificationsJob : BackgroundService
    {
        #region Lifecycle

        private readonly SchedulesCache _schedulesCache;
        private readonly IApplicationRepositoryFactory _repoFactory;
        private readonly NotificationsQueue _notificationsQueue;
        private readonly ILogger<ScheduledNotificationsJob> _logger;
        private readonly ApplicationBehaviorHelper _helper;

        public ScheduledNotificationsJob(
            SchedulesCache schedulesCache,
            IApplicationRepositoryFactory repoFactory,
            NotificationsQueue notificationsQueue,
            ILogger<ScheduledNotificationsJob> logger,
            ApplicationBehaviorHelper helper)
        {
            _schedulesCache = schedulesCache;
            _repoFactory = repoFactory;
            _notificationsQueue = notificationsQueue;
            _logger = logger;
            _helper = helper;
        }

        #endregion

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            _logger.LogInformation(GetType().Name + " Started.");

            // Enter the background job
            while (!cancellation.IsCancellationRequested)
            {
                try
                {
                    ///////////// (1) Create and grab the cancellation token that indicates an updated schedule
                    // It is important to create the token before grabbing the schedules, because
                    // the updater job updates the schedules and THEN cancels the token.
                    var signalSchedulesChange = _schedulesCache.CreateCancellationToken();

                    ///////////// (2) Compute the earliest schedule
                    DateTimeOffset minNext = DateTimeOffset.MaxValue;
                    Dictionary<int, List<ScheduleInfo>> allTemplates = new();

                    var allSchedules = await _schedulesCache.GetSchedules(cancellation);
                    foreach (var schedule in allSchedules.Where(e => !e.IsError))
                    {
                        var tenantId = schedule.TenantId;
                        foreach (var cron in schedule.Crons)
                        {
                            var next = cron.GetNextOccurrence(schedule.LastExecuted, TimeZoneInfo.Utc);
                            if (next != null)
                            {
                                if (minNext.UtcTicks > next.Value.UtcTicks)
                                {
                                    // If it's an earlier time, create a new dictionary
                                    allTemplates = new() { { tenantId, new() { schedule } } };
                                    minNext = next.Value;
                                }
                                else if (minNext.UtcTicks == next.Value.UtcTicks)
                                {
                                    if (!allTemplates.TryGetValue(tenantId, out List<ScheduleInfo> list))
                                    {
                                        allTemplates.Add(tenantId, list = new());
                                    }

                                    list.Add(schedule);
                                }
                            }
                        }
                    }

                    ///////////// (3) Wait the necessary duration if any, or forever if there are no schedules
                    var spanTillNext = minNext - DateTimeOffset.UtcNow;
                    if (spanTillNext.Ticks > 0)
                    {
                        // Combine 1. the token to stop the service with 2. the token to signal schedules changes
                        using var combinedSource = CancellationTokenSource
                            .CreateLinkedTokenSource(cancellation, signalSchedulesChange);

                        // Wait for the time of the notification
                        var milliseconds = Convert.ToInt64(spanTillNext.TotalMilliseconds);
                        await Delay(milliseconds, combinedSource.Token);
                    }

                    ///////////// (4) Run the templates (if they haven't changed)
                    if (!signalSchedulesChange.IsCancellationRequested && !cancellation.IsCancellationRequested)
                    {
                        await Task.WhenAll(allTemplates.Select(async pair =>
                        {
                            // A. Load the templates from the DB
                            // B. IF they are outdated, skip all and update the schedules of this company, then leave it to be handled by the next iteration
                            // C. ELSE run the templates in sequence, and update LastExecuted both in DB and in memory
                            // D. If there is an error evaluating the template or sending it, notify the support email of the company

                            var tenantId = pair.Key;
                            var schedules = pair.Value;

                            try // To prevent failure in one tenant to affect other tenants
                            {

                                // Get deployed automatic templates of this tenant
                                var emailTemplateIds = schedules.Where(e => e.Channel == ScheduleChannel.Email).Select(e => e.TemplateId);
                                var messageTemplateIds = schedules.Where(e => e.Channel == ScheduleChannel.Message).Select(e => e.TemplateId);

                                var repo = _repoFactory.GetRepository(tenantId);
                                var output = await repo.Templates__Load(emailTemplateIds, messageTemplateIds, cancellation);

                                // If the schedules version is stale skip running them
                                var isStale = await _schedulesCache.RefreshSchedulesIfStale(tenantId, output.SchedulesVersion, cancellation);
                                if (!isStale)
                                {
                                    foreach (var template in output.EmailTemplates)
                                    {
                                        // TODO
                                    }

                                    foreach (var template in output.MessageTemplates)
                                    {
                                        await _schedulesCache.UpdateMessageTemplateLastExecuted(tenantId, template.Id, minNext, output.SupportEmails, async () =>
                                        {
                                            // (1) Prepare the Message
                                            var preview = await _helper.CreateMessageCommandPreview(
                                                tenantId: tenantId,
                                                userId: 0, // Irrelevant
                                                settingsVersion: output.SettingsVersion,
                                                userSettingsVersion: null, // Irrelevant
                                                template: template,
                                                preloadedQuery: null,
                                                localVariables: null,
                                                cultureString: "en", // TODO culture?
                                                cancellation: cancellation);

                                            // Prepare the messages
                                            if (preview.Messages.Count > 0)
                                            {
                                                var messagesToSend = preview.Messages.Select(msg => new SmsToSend(
                                                    phoneNumber: msg.PhoneNumber,
                                                    content: msg.Content)
                                                ).ToList();

                                                var command = new NotificationCommandToSend(template.Id)
                                                {
                                                    Caption = preview.Caption,
                                                    ScheduledTime = minNext
                                                };

                                                // (2) Send Messages
                                                // await _notificationsQueue.Enqueue(tenantId, smsMessages: messagesToSend, command: command, cancellation: cancellation);
                                                foreach (var msg in messagesToSend)
                                                    _logger.LogDebug($"{minNext.LocalDateTime}: {msg.PhoneNumber}: {msg.Content}");
                                            }
                                        },
                                        cancellation);
                                    }
                                }
                            }
                            catch (TaskCanceledException) { }
                            catch (OperationCanceledException) { }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error in {GetType().Name} while running the templates of tenant Id {tenantId}.");
                            }
                        }));
                    }
                }
                catch (TaskCanceledException) { }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in {GetType().Name}.");
                    await Task.Delay(60 * 1000, cancellation); // Wait a minute to prevent a tight infinite loop
                }
            }

            _schedulesCache.DisposeCurrentToken();
        }

        /// <summary>
        /// Similar to <see cref="Task.Delay(int, CancellationToken)"/> but is able to handle 
        /// a <paramref name="milliseconds"/> value larger than int.MaxValue.
        /// </summary>
        /// <remarks>
        /// Does not throw <see cref="TaskCanceledException"/> when <paramref name="cancellation"/>
        /// is canceled.
        /// </remarks>
        /// <param name="milliseconds">How many milliseconds to delay.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        static async Task Delay(long milliseconds, CancellationToken cancellation)
        {
            while (milliseconds > 0 && !cancellation.IsCancellationRequested)
            {
                var currentDelay = milliseconds > int.MaxValue ? int.MaxValue : (int)milliseconds;
                milliseconds -= currentDelay;

                try
                {
                    await Task.Delay(currentDelay, cancellation);
                }
                catch (TaskCanceledException) { };
            }
        }
    }

    public class SchedulesCache
    {
        private readonly IApplicationRepositoryFactory _repoFactory;
        private readonly ILogger<SchedulesCache> _logger;
        private readonly Dictionary<int, CacheEntry> _cache = new(); // tenantId => CacheEntry
        private readonly object _cacheLock = new();
        private readonly object _tokenLock = new();

        private CancellationTokenSource _tokenSource = new();

        public SchedulesCache(IApplicationRepositoryFactory repoFactory, ILogger<SchedulesCache> logger)
        {
            _repoFactory = repoFactory;
            _logger = logger;
        }

        /// <summary>
        /// Returns all schedules of all adopted tenants in a thread-safe manner.
        /// </summary>
        /// <param name="cancellation">The cancellation instruction.</param>
        public async Task<IEnumerable<ScheduleInfo>> GetSchedules(CancellationToken cancellation)
        {
            var result = new ConcurrentBag<ScheduleInfo>();

            // Grab all the entries in a thread-safe manner
            IEnumerable<CacheEntry> entries;
            lock (_cacheLock)
            {
                entries = _cache.Values.ToList();
            }

            await Task.WhenAll(entries.Select(async entry =>
            {
                await entry.Semaphore.WaitAsync(cancellation);
                try
                {
                    foreach (var schedule in entry.EmailSchedules.Values)
                    {
                        result.Add(schedule);
                    }

                    foreach (var schedule in entry.MessageSchedules.Values)
                    {
                        result.Add(schedule);
                    }
                }
                finally
                {
                    // Very important
                    entry.Semaphore.Release();
                }
            }));

            return result;
        }

        //public async Task UpdateEmailTemplateLastExecuted(
        //    int tenantId,
        //    IEnumerable<int> templateIds,
        //    DateTimeOffset value,
        //    Func<Task> sendEmails, CancellationToken cancellation)
        //{
        //    var repo = _repoFactory.GetRepository(tenantId);

        //    // Start transaction

        //    // Grab the entry in a thread-safe manner
        //    CacheEntry entry = GetCacheEntry(tenantId);
        //    await entry.Semaphore.WaitAsync(cancellation);
        //    try
        //    {
        //        using var trx = TransactionFactory.ReadCommitted(TransactionScopeOption.RequiresNew); // Just to allow rolling back

        //        await repo.EmailTemplates__UpdateLastExecuted(templateIds, value, cancellation); // Transactional operations first
        //        await sendEmails(); // Followed by non-transactional operations

        //        trx.Complete();

        //        foreach (var templateId in templateIds)
        //        {
        //            if (entry.EmailSchedules.TryGetValue(templateId, out ScheduleInfo schedule))
        //            {
        //                schedule.LastExecuted = value;
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        // Very important
        //        entry.Semaphore.Release();
        //    }
        //}

        public async Task UpdateMessageTemplateLastExecuted(
            int tenantId,
            int templateId,
            DateTimeOffset lastExecuted,
            string supportEmails,
            Func<Task> sendMessages,
            CancellationToken cancellation)
        {
            var repo = _repoFactory.GetRepository(tenantId);

            bool isCriticalError = false;

            // Grab the entry in a thread-safe manner
            CacheEntry entry = GetCacheEntry(tenantId);
            await entry.Semaphore.WaitAsync(cancellation);
            try
            {
                // Start transaction
                using var trx = TransactionFactory.ReadCommitted(TransactionScopeOption.RequiresNew); // Just to allow rolling back

                await repo.MessageTemplates__UpdateLastExecuted(templateId, lastExecuted, cancellation); // Transactional operations first
                await sendMessages(); // Followed by non-transactional operations

                trx.Complete();

                if (entry.MessageSchedules.TryGetValue(templateId, out ScheduleInfo schedule))
                {
                    schedule.LastExecuted = lastExecuted;
                }
            }
            catch (ReportableException ex) // This exception is caused by a bug in the template itself
            {
                try
                {
                    await repo.MessageTemplates__SetIsError(templateId, cancellation); // Transactional operations first

                    if (entry.MessageSchedules.TryGetValue(templateId, out ScheduleInfo schedule))
                    {
                        schedule.IsError = true;
                    }

                    // TODO: notify supportEmails
                    _logger.LogError($"Email Notification to {supportEmails}... " + ex.Message);
                }
                catch { }
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {GetType().Name} while running the template with Id {templateId} in tenant Id = {tenantId}.");

                isCriticalError = true;
            }
            finally
            {
                // Very important
                entry.Semaphore.Release();
            }

            // To prevent a tight infinite loop when there is a critical error
            if (isCriticalError)
            {
                await Task.Delay(45 * 1000, cancellation);
            }
        }

        /// <summary>
        /// Retrieves the <see cref="CacheEntry"/> associated with this Tenant Id in a thread-safe manner.
        /// </summary>
        private CacheEntry GetCacheEntry(int tenantId)
        {
            // Grab the entry in a thread-safe manner
            CacheEntry entry;
            lock (_cacheLock)
            {
                if (!_cache.TryGetValue(tenantId, out entry))
                {
                    _cache.Add(tenantId, entry = new CacheEntry { TenantId = tenantId });
                }
            }

            return entry;
        }

        /// <summary>
        /// Checks if the cached version is equal to the supplied <paramref name="version"/>. 
        /// If they are different it updates the cache from the source and returns TRUE, 
        /// otherwise does nothing and returns false.
        /// </summary>
        /// <param name="tenantId">The tenantId whose cache we're updating.</param>
        /// <param name="version">The value of the latest fresh version.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>TRUE if the cache was stale, and FALSE otherwise.</returns>
        public async Task<bool> RefreshSchedulesIfStale(int tenantId, string version, CancellationToken cancellation)
        {
            bool isStale = false; // Will be returned

            // Grab the entry in a thread-safe manner
            CacheEntry entry = GetCacheEntry(tenantId);

            // Update if stale
            if (entry.Version != version)
            {
                // The cache is stale => we need to refresh the cache.
                // Use the semaphore to make sure only one thread is
                // refreshing the cache while the others await.
                await entry.Semaphore.WaitAsync(cancellation);
                try
                {
                    // A second OCD-check inside the semaphore block
                    if (entry.Version != version)
                    {
                        isStale = true;

                        try
                        {
                            var repo = _repoFactory.GetRepository(tenantId);
                            var output = await repo.Schedules__Load(cancellation);

                            string freshVersion = output.SchedulesVersion;
                            Dictionary<int, ScheduleInfo> emailSchedules = new();
                            Dictionary<int, ScheduleInfo> messageSchedules = new();

                            foreach (var template in output.EmailTemplates)
                            {
                                try
                                {
                                    emailSchedules.Add(template.Id, new ScheduleInfo(
                                        tenantId: tenantId,
                                        channel: ScheduleChannel.Email,
                                        templateId: template.Id,
                                        version: output.SchedulesVersion,
                                        crons: Parse(template.Schedule),
                                        lastExecuted: template.LastExecuted ?? throw new InvalidOperationException($"Scheduling error: {nameof(NotificationTemplate)} with Id {template.Id} has {nameof(NotificationTemplate.LastExecuted)} = NULL."),
                                        isError: template.IsError ?? throw new InvalidOperationException($"Scheduling error: {nameof(NotificationTemplate)} with Id {template.Id} has {nameof(NotificationTemplate.IsError)} = NULL.")
                                        ));
                                }
                                catch (CronFormatException ex)
                                {
                                    _logger.LogError(ex, $"Error in {GetType().Name} while parsing CRON expression {template.Schedule} for email template with Id={template.Id}.");
                                }
                            }

                            foreach (var template in output.MessageTemplates)
                            {
                                try
                                {
                                    messageSchedules.Add(template.Id, new ScheduleInfo(
                                        tenantId: tenantId,
                                        channel: ScheduleChannel.Message,
                                        templateId: template.Id,
                                        version: output.SchedulesVersion,
                                        crons: Parse(template.Schedule),
                                        lastExecuted: template.LastExecuted ?? throw new InvalidOperationException($"Scheduling error: {nameof(MessageTemplate)} with Id {template.Id} has {nameof(MessageTemplate.LastExecuted)} = NULL."),
                                        isError: template.IsError ?? throw new InvalidOperationException($"Scheduling error: {nameof(MessageTemplate)} with Id {template.Id} has {nameof(MessageTemplate.IsError)} = NULL.")
                                        ));
                                }
                                catch (CronFormatException ex)
                                {
                                    _logger.LogError(ex, $"Error in {GetType().Name} while parsing CRON expression {template.Schedule} for message template with Id={template.Id}.");
                                }
                            }

                            entry.EmailSchedules = emailSchedules;
                            entry.MessageSchedules = messageSchedules;
                            entry.Version = freshVersion;
                        }
                        catch (TaskCanceledException) { }
                        catch (OperationCanceledException) { }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error in {GetType().Name} while loading schedules for tenantId {tenantId}.");

                            // Failed to load the schedules for whatever reason, put empty lists and a fake version to try again next time.
                            entry.EmailSchedules = new();
                            entry.MessageSchedules = new();
                            entry.Version = Guid.NewGuid().ToString();
                        }
                    }
                }
                finally
                {
                    // Very important
                    entry.Semaphore.Release();
                }
            }

            return isStale;
        }

        private static IEnumerable<CronExpression> Parse(string schedule)
        {
            return schedule
                .Split(';')
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim())
                .Select(CronExpression.Parse);
        }

        /// <summary>
        /// Dispose the current <see cref="CancellationToken"/> and creates and returns a new one in a thread-safe manner.
        /// The token is canceled when the schedules of any tenant change.
        /// </summary>
        public CancellationToken CreateCancellationToken()
        {
            lock (_tokenLock)
            {
                _tokenSource.Dispose();
                _tokenSource = new();
                return _tokenSource.Token;
            }
        }

        /// <summary>
        /// Cancel the current <see cref="CancellationToken"/> in a thread-safe manner.
        /// The token is canceled when the schedules of any tenant change.
        /// </summary>
        public void CancelCurrentToken()
        {
            lock (_tokenLock)
            {
                _tokenSource.Cancel();
            }
        }

        /// <summary>
        /// Dispose the current <see cref="CancellationToken"/> in a thread-safe manner.
        /// The token is disposed when the server is shutting down.
        /// </summary>
        public void DisposeCurrentToken()
        {
            lock (_tokenLock)
            {
                _tokenSource.Dispose();
            }
        }

        private class CacheEntry
        {
            public SemaphoreSlim Semaphore { get; } = new(initialCount: 1);

            public string Version { get; set; } = Guid.NewGuid().ToString();

            public Dictionary<int, ScheduleInfo> EmailSchedules { get; set; }

            public Dictionary<int, ScheduleInfo> MessageSchedules { get; set; }

            public int TenantId { get; set; }
        }
    }

    public class SchedulesUpdaterJob : BackgroundService
    {
        #region Lifecycle

        private readonly InstanceInfoProvider _instanceInfo;
        private readonly SchedulesCache _cache;
        private readonly IApplicationRepositoryFactory _repoFactory;
        private readonly ILogger<SchedulesUpdaterJob> _logger;

        public SchedulesUpdaterJob(
            InstanceInfoProvider instanceInfo,
            SchedulesCache cache,
            IApplicationRepositoryFactory repoFactory,
            ILogger<SchedulesUpdaterJob> logger)
        {
            _instanceInfo = instanceInfo;
            _cache = cache;
            _repoFactory = repoFactory;
            _logger = logger;
        }

        #endregion

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            _logger.LogInformation(GetType().Name + " Started.");

            while (!cancellation.IsCancellationRequested)
            {
                try
                {
                    // Grab a hold of a concrete list of adopted tenantIds at the current moment
                    var tenantIds = _instanceInfo.AdoptedTenantIds;
                    await Task.WhenAll(tenantIds.Select(async tenantId =>
                    {
                        // (1) Retrieve the schedules version
                        // If we fail to retrieve the version, we will refresh the cache to empty it anyways
                        var version = Guid.NewGuid().ToString();
                        try
                        {
                            var repo = _repoFactory.GetRepository(tenantId);
                            version = await repo.SchedulesVersion__Load(cancellation);
                        }
                        catch (TaskCanceledException) { }
                        catch (OperationCanceledException) { }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error in {GetType().Name} while loading the schedule version for tenant Id = {tenantId}.");
                        }

                        // (2) Use the schedules version to ensure freshness of the cache
                        // This call does not throw an exception
                        var isStale = await _cache.RefreshSchedulesIfStale(tenantId, version, cancellation);
                        if (isStale)
                        {
                            // Stale schedules in the cache => Notify the main job
                            _cache.CancelCurrentToken();
                        }
                    }));
                }
                catch (TaskCanceledException) { }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in {GetType().Name}.");
                }

                // Go to sleep for 1 minute
                await Task.Delay(1000 * 60, cancellation);
            }
        }
    }

    public class ScheduleInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleInfo"/> class.
        /// </summary>
        public ScheduleInfo(
            int tenantId,
            ScheduleChannel channel,
            int templateId,
            string version,
            IEnumerable<CronExpression> crons,
            DateTimeOffset lastExecuted,
            bool isError)
        {
            TenantId = tenantId;
            Channel = channel;
            TemplateId = templateId;
            Version = version;
            Crons = crons?.ToList();
            LastExecuted = lastExecuted;
            IsError = isError;
        }

        /// <summary>
        /// The tenant Id to which this schedule belongs.
        /// </summary>
        public int TenantId { get; }

        /// <summary>
        /// Email or SMS.
        /// </summary>
        public ScheduleChannel Channel { get; }

        /// <summary>
        /// The Id of the template.
        /// </summary>
        public int TemplateId { get; }

        /// <summary>
        /// The version of the current schedules
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// The CRON expression that determines when to run this notification template.
        /// </summary>
        public IEnumerable<CronExpression> Crons { get; }

        /// <summary>
        /// The time when this template was last run.
        /// </summary>
        public DateTimeOffset LastExecuted { get; set; }

        /// <summary>
        /// True if the last run of this template resulted in a <see cref="ReportableException"/>.
        /// </summary>
        public bool IsError { get; set; }
    }

    /// <summary>
    /// Determines whether the schedule pertains to an Email or a Message template.
    /// </summary>
    public enum ScheduleChannel
    {
        Email,
        Message
    }
}
