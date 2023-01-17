using AsyncKeyedLock;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Client
{
    public class SingleThreadedScope
    {
        private static readonly AsyncKeyedLocker<string> _asyncKeyedLocker = new AsyncKeyedLocker<string>(o =>
        {
            o.PoolSize = 20;
            o.PoolInitialFill = 1;
        });

        public static async ValueTask<IDisposable> Create(string name, CancellationToken cancellation = default)
        {
            return await _asyncKeyedLocker.LockAsync(name, cancellation).ConfigureAwait(false);
        }
    }
}
