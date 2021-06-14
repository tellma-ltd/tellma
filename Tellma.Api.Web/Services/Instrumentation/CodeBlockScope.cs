using System;
using System.Diagnostics;

namespace Tellma.Services
{
    public class CodeBlockScope : IDisposable
    {
        private readonly Stopwatch _sw = new Stopwatch();
        private readonly Stopwatch _overhead;
        private readonly Action<long> _onDispose;
        private bool _disposed = false;

        public CodeBlockScope(Stopwatch overhead, Action<long> onDispose)
        {
            _overhead = overhead;
            _onDispose = onDispose;

            _sw.Start();
        }

        public void Dispose()
        {
            _overhead.Start();

            if (_disposed)
            {
                return; // Dispose a block only once
            }

            _disposed = true;

            _sw.Stop();
            _onDispose(_sw.ElapsedMilliseconds);

            _overhead.Stop();
        }
    }
}
