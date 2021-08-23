using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Api.Notifications
{
    /// <summary>
    /// Base class for several queues that allow for efficient "fire and forget" operations.
    /// I.e. some user initiated operation queues an item, then a background job dequeues and
    /// handles it asynchrously without blocking the user operation. This code is modified
    /// from https://bit.ly/2EvjA58, it is thread-safe and allows "awaiting" new items.
    /// </summary>
    /// <typeparam name="TItem">The type of queued items.</typeparam>
    public class BackgroundQueue<TItem>
    {
        private readonly ConcurrentQueue<(TItem item, DateTimeOffset queuedAt)> _queue = new();
        private readonly SemaphoreSlim _signal = new(0);

        public virtual void QueueBackgroundWorkItem(TItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _queue.Enqueue((item, DateTimeOffset.Now));
            _signal.Release();
        }

        public void QueueAllBackgroundWorkItems(IEnumerable<TItem> items)
        {
            foreach (var item in items)
            {
                QueueBackgroundWorkItem(item);
            }
        }

        public async virtual Task<(TItem item, DateTimeOffset queuedAt)> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);

            _queue.TryDequeue(out var item);

            return item;
        }
    }
}
