using System.Collections.Generic;
using System.Linq;
using Tellma.Utilities.Email;

namespace Tellma.Api.Notifications
{
    public class EmailQueue : BackgroundQueue<IEnumerable<EmailToSend>>
    {
        private const int ChunkSize = 100;
        public override void QueueBackgroundWorkItem(IEnumerable<EmailToSend> emailBatch)
        {
            // This protects against very big batches of emails, by splitting them into small chunks of 100 emails each
            int skip = 0;
            while (true)
            {
                var chunk = emailBatch.Skip(skip).Take(ChunkSize);
                if (chunk.Any())
                {
                    base.QueueBackgroundWorkItem(chunk);
                    skip += ChunkSize;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
