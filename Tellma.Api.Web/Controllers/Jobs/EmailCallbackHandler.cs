using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Data;
using Tellma.Model.Application;
using Tellma.Services.Email;

namespace Tellma.Controllers.Jobs
{
    public class EmailCallbackHandler : IEmailCallbackHandler
    {
        private readonly ApplicationRepositoryLite _repo;

        public EmailCallbackHandler(ApplicationRepositoryLite repo)
        {
            _repo = repo;
        }

        public async Task HandleCallback(IEnumerable<EmailEventNotification> emailEvents, CancellationToken cancellation)
        {
            // Group events by tenant Id
            var emailEventsByTenant = emailEvents
                .Where(e => e.TenantId != null && e.TenantId != 0) // Right now we do not handle null or zero tenant Ids, those were probably sent from identity or admin servers
                .GroupBy(e => e.TenantId.Value);

            // Run all tenants in parallel
            await Task.WhenAll(emailEventsByTenant.Select(async emailEventsOfTenant =>
            {
                var tenantId = emailEventsOfTenant.Key;

                var stateUpdatesOfTenant = emailEventsOfTenant.Select(emailEvent => new IdStateErrorTimestamp
                {
                    Id = emailEvent.EmailId,
                    Error = emailEvent.Error,
                    Timestamp = emailEvent.Timestamp,
                    State = emailEvent.Event switch
                    {
                        EmailEvent.Dropped => EmailState.DispatchFailed,
                        EmailEvent.Delivered => EmailState.Delivered,
                        EmailEvent.Bounce => EmailState.DeliveryFailed,
                        EmailEvent.Open => EmailState.Opened,
                        EmailEvent.Click => EmailState.Clicked,
                        EmailEvent.SpamReport => EmailState.ReportedSpam,
                        _ => throw new InvalidOperationException($"[Bug] Unknown {nameof(EmailEvent)} = {emailEvent.Event}"), // Future proofing
                    }
                });

                // Update the state in the database (should we make it serializable?)
                await _repo.Notifications_Emails__UpdateState(tenantId, stateUpdatesOfTenant, cancellation);
            }));
        }
    }
}
