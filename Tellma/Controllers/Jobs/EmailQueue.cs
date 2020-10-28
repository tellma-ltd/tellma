using System.Collections.Generic;
using System.Linq;
using Tellma.Services.Email;

namespace Tellma.Controllers.Jobs
{
    public class EmailQueue : BackgroundQueue<IEnumerable<Email>>
    {
        private const int BatchSize = 100;
        public override void QueueBackgroundWorkItem(IEnumerable<Email> emails)
        {
            // This protects against huge number of emails, by batching them in chunks of 100 emails each
            while (true)
            {
                int skip = 0;
                var batch = emails.Skip(skip).Take(BatchSize);

                if (batch.Any())
                {
                    base.QueueBackgroundWorkItem(batch);
                }
                else
                {
                    break;
                }
            }
        }
    }
}
