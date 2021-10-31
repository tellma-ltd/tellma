using System.Collections.Generic;
using System.Linq;
using Tellma.Utilities.Email;

namespace Tellma.Api.Notifications
{
    public class EmailQueue : BackgroundQueue<IEnumerable<EmailToSend>>
    {
        private const int ChunkSize = 200;
        public override void QueueBackgroundWorkItem(IEnumerable<EmailToSend> emailBatch)
        {
            // This protects against very big batches of emails, by splitting them into smaller chunks
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
