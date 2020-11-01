using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Data;
using Tellma.Entities;
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
            var emailEventsByTenant = emailEvents.Where(e => e.TenantId != null).GroupBy(e => e.TenantId.Value);

            // Run all tenants in parallel
            await Task.WhenAll(emailEventsByTenant.Select(async emailEventsOfTenant =>
            {
                var tenantId = emailEventsOfTenant.Key;

                var stateUpdates = new List<IdStateError>();
                var engagementUpdates = new List<IdStateError>();

                foreach (var emailEvent in emailEventsOfTenant)
                {
                    // Map the event to the database representation
                    var state = emailEvent.Event switch
                    {
                        EmailEvent.Dropped => EmailState.DispatchFailed,
                        EmailEvent.Delivered => EmailState.Delivered,
                        EmailEvent.Bounce => EmailState.DeliveryFailed,
                        EmailEvent.Open => EmailEngagementState.Opened,
                        EmailEvent.Click => EmailEngagementState.Clicked,
                        EmailEvent.SpamReport => EmailEngagementState.ReportedSpam,
                        _ => throw new InvalidOperationException($"[Bug] Unknown {nameof(EmailEvent)} = {emailEvent.Event}"), // Future proofing
                    };

                    // Create the update
                    var update = new IdStateError
                    {
                        Id = emailEvent.EmailId,
                        State = state,
                        Error = emailEvent.Error
                    };

                    // Distinguish whether this is a state update or an engagement update
                    switch (emailEvent.Event)
                    {
                        case EmailEvent.Dropped:
                        case EmailEvent.Delivered:
                        case EmailEvent.Bounce:
                            stateUpdates.Add(update);
                            break;

                        case EmailEvent.Open:
                        case EmailEvent.Click:
                        case EmailEvent.SpamReport:
                            engagementUpdates.Add(update);
                            break;
                        default:
                            throw new InvalidOperationException($"[Bug] Unknown {nameof(EmailEvent)} = {emailEvent.Event}"); // Future proofing
                    }
                }

                // Update the state in the database (should we make it serializable?)
                await _repo.Notifications_Emails__UpdateState(tenantId, stateUpdates, engagementUpdates, cancellation);
            }));
        }
    }
}
