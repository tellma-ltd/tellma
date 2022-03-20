using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Behaviors;
using Tellma.Repository.Application;
using Tellma.Utilities.Email;
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
                                var emailTemplateIds = schedules.Where(e => e.Channel == ScheduleChannel.Email).Select(e => e.TemplateId).Distinct();
                                var messageTemplateIds = schedules.Where(e => e.Channel == ScheduleChannel.Message).Select(e => e.TemplateId).Distinct();

                                var repo = _repoFactory.GetRepository(tenantId);
                                var output = await repo.Templates__Load(emailTemplateIds, messageTemplateIds, cancellation);

                                // If the schedules version is stale skip running them
                                var isStale = await _schedulesCache.RefreshSchedulesIfStale(tenantId, output.SchedulesVersion, cancellation);
                                if (!isStale)
                                {
                                    foreach (var template in output.EmailTemplates)
                                    {
                                        await _schedulesCache.UpdateEmailTemplateLastExecuted(tenantId, template.Id, minNext, output.SupportEmails, async () =>
                                        {
                                            // (1) Prepare the Email
                                            var preview = await _helper.CreateEmailCommandPreview(
                                                tenantId: tenantId,
                                                userId: 0, // Irrelevant
                                                settingsVersion: output.SettingsVersion,
                                                userSettingsVersion: null, // Irrelevant
                                                template: template,
                                                preloadedQuery: null,
                                                localVariables: null,
                                                fromIndex: 0,
                                                toIndex: int.MaxValue,
                                                cultureString: "en", // TODO culture?
                                                now: minNext,
                                                isAnonymous: true, // Bypasses permission check
                                                getReadPermissions: null,
                                                cancellation: cancellation);

                                            // (2) Send Emails
                                            if (preview.Emails.Count > 0)
                                            {
                                                var emailsToSend = preview.Emails.Select(email => new EmailToSend
                                                {
                                                    TenantId = tenantId,
                                                    To = email.To,
                                                    Subject = email.Subject,
                                                    Cc = email.Cc,
                                                    Bcc = email.Bcc,
                                                    Body = email.Body,
                                                    Attachments = email.Attachments.Select(e => new EmailAttachmentToSend
                                                    {
                                                        Name = e.DownloadName,
                                                        Contents = Encoding.UTF8.GetBytes(e.Body)
                                                    }).ToList()
                                                }).ToList();

                                                var command = new EmailCommandToSend(template.Id)
                                                {
                                                    Caption = preview.Caption,
                                                    ScheduledTime = minNext
                                                };

                                                await _notificationsQueue.Enqueue(tenantId, emails: emailsToSend, command: command, cancellation: cancellation);
                                                //foreach (var email in emailsToSend)
                                                //{
                                                //    _logger.LogInformation($"{minNext.LocalDateTime}: {string.Join("; ", email.To)}: {email.Subject}");
                                                //}
                                            }
                                        },
                                        cancellation);
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
                                                now: minNext,
                                                isAnonymous: true, // Bypasses permission check
                                                getReadPermissions: null,
                                                cancellation: cancellation);

                                            // (2) Send Messages
                                            if (preview.Messages.Count > 0)
                                            {
                                                var messagesToSend = preview.Messages.Select(msg => new SmsToSend(
                                                    phoneNumber: msg.PhoneNumber,
                                                    content: msg.Content)
                                                {
                                                    TenantId = tenantId
                                                }).ToList();

                                                var command = new EmailCommandToSend(template.Id)
                                                {
                                                    Caption = preview.Caption,
                                                    ScheduledTime = minNext
                                                };

                                                await _notificationsQueue.Enqueue(tenantId, smsMessages: messagesToSend, command: command, cancellation: cancellation);
                                                //foreach (var msg in messagesToSend)
                                                //{
                                                //    _logger.LogInformation($"{minNext.LocalDateTime}: {msg.PhoneNumber}: {msg.Content}");
                                                //}
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
                                await Task.Delay(30 * 1000, cancellation); // Wait 30 seconds to prevent a tight infinite loop
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
}
