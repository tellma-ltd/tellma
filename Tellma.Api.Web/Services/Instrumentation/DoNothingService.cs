using System;

namespace Tellma.Services.Instrumentation
{
    public class DoNothingService : IInstrumentationService
    {
        private readonly DoNothingScope _doNothing = new DoNothingScope();
        public IDisposable Block(string name)
        {
            return _doNothing;
        }

        public IDisposable Disable()
        {
            return _doNothing;
        }

        public InstrumentationReport GetReport()
        {
            return null;
        }

        public void NextMiddleware(string _)
        {
        }
    }
}
