using Cronos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Model.Application;
using Tellma.Repository.Application;
using Tellma.Utilities.Common;
using Tellma.Utilities.Email;

namespace Tellma.Api.Notifications
{
    public class SchedulesCache
    {
        private readonly IApplicationRepositoryFactory _repoFactory;
        private readonly ILogger<SchedulesCache> _logger;
        private readonly NotificationsQueue _queue;
        private readonly Dictionary<int, CacheEntry> _cache = new(); // tenantId => CacheEntry
        private readonly object _cacheLock = new();
        private readonly object _tokenLock = new();

        private CancellationTokenSource _tokenSource = new();

        public SchedulesCache(IApplicationRepositoryFactory repoFactory, ILogger<SchedulesCache> logger, NotificationsQueue queue)
        {
            _repoFactory = repoFactory;
            _logger = logger;
            _queue = queue;
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

        public async Task UpdateEmailTemplateLastExecuted(
          int tenantId,
          int templateId,
          DateTimeOffset lastExecuted,
          string supportEmails,
          Func<Task> sendEmails,
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

                await repo.EmailTemplates__UpdateLastExecuted(templateId, lastExecuted, cancellation); // Transactional operations first
                await sendEmails(); // Followed by non-transactional operations

                trx.Complete();

                if (entry.EmailSchedules.TryGetValue(templateId, out ScheduleInfo schedule))
                {
                    schedule.LastExecuted = lastExecuted;
                }
            }
            catch (ReportableException ex) // This exception is caused by a bug in the template itself
            {
                try
                {
                    await repo.EmailTemplates__SetIsError(templateId, cancellation); // Transactional operations first

                    if (entry.EmailSchedules.TryGetValue(templateId, out ScheduleInfo schedule))
                    {
                        schedule.IsError = true;
                    }

                    // TODO: notify supportEmails
                    if (!string.IsNullOrWhiteSpace(supportEmails))
                    {
                        var supportEmailsEnum = supportEmails
                            .Split(";")
                            .Select(e => e.Trim())
                            .Where(e => !string.IsNullOrWhiteSpace(e));

                        await _queue.Enqueue(tenantId, new List<EmailToSend>
                        {
                           new EmailToSend
                           {
                                Subject = "Error Executing Email Template",
                                To = supportEmailsEnum,
                                Body = ex.Message
                           }
                        }, cancellation: default);
                    }
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
                    if (!string.IsNullOrWhiteSpace(supportEmails))
                    {
                        var supportEmailsEnum = supportEmails
                            .Split(";")
                            .Select(e => e.Trim())
                            .Where(e => !string.IsNullOrWhiteSpace(e));

                        await _queue.Enqueue(tenantId, new List<EmailToSend>
                        {
                           new EmailToSend
                           {
                                Subject = "Error Executing Message Template",
                                To = supportEmailsEnum,
                                Body = ex.Message
                           }
                        }, cancellation: default);
                    }
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
                                        lastExecuted: template.LastExecuted ?? throw new InvalidOperationException($"Scheduling error: {nameof(EmailTemplate)} with Id {template.Id} has {nameof(EmailTemplate.LastExecuted)} = NULL."),
                                        isError: template.IsError ?? throw new InvalidOperationException($"Scheduling error: {nameof(EmailTemplate)} with Id {template.Id} has {nameof(EmailTemplate.IsError)} = NULL.")
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
}
