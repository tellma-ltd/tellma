using System;
using Tellma.Services.Instrumentation;

namespace Tellma.Services
{
    public interface IInstrumentationService
    {
        IDisposable Block(string name);

        void NextMiddleware(string name);

        InstrumentationReport GetReport();
    }
}