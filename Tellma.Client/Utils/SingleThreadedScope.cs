using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Client
{
    public class SingleThreadedScope : IDisposable
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
        private readonly string _name;

        /// <summary>
        /// Can only create an instance using the Create method.
        /// </summary>
        private SingleThreadedScope(string name)
        {
            _name = name;
        }

        public static async Task<SingleThreadedScope> Create(string name, CancellationToken cancellation = default)
        {
            var semaphore = _semaphores.GetOrAdd(name, _ => new SemaphoreSlim(1));
            await semaphore.WaitAsync(cancellation);

            return new SingleThreadedScope(name);
        }

        public void Dispose()
        {
            if (_semaphores.TryGetValue(_name, out SemaphoreSlim semaphore))
            {
                semaphore.Release();
            }
            else
            {
                // Should never reach here
                throw new InvalidOperationException($"[Bug] Could not find semaphore with name {_name}.");
            }
        }
    }
}
