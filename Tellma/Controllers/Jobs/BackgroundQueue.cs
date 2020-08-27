using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Controllers.Jobs
{
    /// <summary>
    /// Base class for several queues that allow for efficient "fire and forget" operations.
    /// I.e. some user initiated operation queus an item, then a background job dequeues and
    /// handles it asynchrously without blocking the user operation. This code is modified
    /// from https://bit.ly/2EvjA58, it is thread-safe and allows "awaiting" new items.
    /// </summary>
    /// <typeparam name="T">The type of queued items</typeparam>
    public class BackgroundQueue<T>
    {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void QueueBackgroundWorkItem(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _queue.Enqueue(item);
            _signal.Release();
        }

        public async Task<T> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);

            _queue.TryDequeue(out var item);

            return item;
        }
    }
}
