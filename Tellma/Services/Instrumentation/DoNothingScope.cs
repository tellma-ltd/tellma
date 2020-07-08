using System;

namespace Tellma.Services.Instrumentation
{
    /// <summary>
    /// Used when instrumentation is disabled
    /// </summary>
    public class DoNothingScope : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
