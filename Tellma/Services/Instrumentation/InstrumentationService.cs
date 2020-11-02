using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tellma.Services.Instrumentation;

namespace Tellma.Services.Instrumentation
{
    public class InstrumentationService : IInstrumentationService
    {
        private readonly long _threshold;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        public InstrumentationService(IOptions<InstrumentationOptions> options)
        {
            _threshold = options?.Value?.ThresholdInMilliseconds ?? 500L; // Default to 500 milliseconds
            _lifetimeSw.Start();
            _middlewareSw.Start();
        }

        private readonly Stopwatch _lifetimeSw = new Stopwatch();
        private readonly Stopwatch _middlewareSw = new Stopwatch();
        private readonly List<CodeBlockInstrumentation> _middlewareInstrumentation = new List<CodeBlockInstrumentation>();

        /// <summary>
        /// This measures the overhead of collecting the instrumentation, to ensure it's not too much
        /// </summary>
        private readonly Stopwatch _overhead = new Stopwatch();

        /// <summary>
        /// Stack to keep track of ancestors when we descend into sub code blocks
        /// </summary>
        private readonly Stack<CodeBlockInstrumentation> _stack = new Stack<CodeBlockInstrumentation>();

        /// <summary>
        /// Keeps track of the currently active code block.
        /// Note: the root <see cref="CodeBlockInstrumentation"/> instance is merely a container for
        /// the collection of actual root blocks, it won't make it into the report.
        /// </summary>
        private CodeBlockInstrumentation _current = new CodeBlockInstrumentation(); // The root

        /// <summary>
        /// Called at the beginning of a code block that we intend to measure the performance of,
        /// the returned <see cref="IDisposable"/> should be called at the end of that code block
        /// for the measurement to persist, failure to do so will result in an exception when calling
        /// <see cref="GetReport"/>
        /// </summary>
        public IDisposable Block(string name)
        {
            _overhead.Start();

            // Add (or retrieve) a sub-block
            var subBlock = _current.AddSubBlock(name);

            // Push the state in the stack
            _stack.Push(_current);
            _current = subBlock;

            var scope = new CodeBlockScope(_overhead, onDispose: (long time) =>
            {
                subBlock.T += time;

                // Pop the stack when we're done with a block
                _current = _stack.Pop();
            });

            _overhead.Stop();
            return scope;
        }

        /// <summary>
        /// Returns the final report that can be served with the response
        /// </summary>
        /// <returns></returns>
        public InstrumentationReport GetReport()
        {
            // If the lifetime is below the threshold, don't return any instrumentation
            _lifetimeSw.Stop();
            var totalTime = _lifetimeSw.ElapsedMilliseconds;
            if (totalTime < _threshold)
            {
                return null;
            }

            // Keep popping until you hit the root
            while (_stack.Count > 0)
            {
                throw new InvalidOperationException("Bug: Some instrumentation scopes have not been disposed");
            }

            // Return the result
            return new InstrumentationReport
            {
                Middleware = _middlewareInstrumentation,
                Breakdown = _current.B,
                Overhead = _overhead.ElapsedMilliseconds,
                Total = totalTime, 
            };
        }

        public void NextMiddleware(string name)
        {
            var milliseconds = _middlewareSw.ElapsedMilliseconds;
            _middlewareSw.Restart();

            _middlewareInstrumentation.Add(new CodeBlockInstrumentation
            {
                N = name,
                T = milliseconds
            });
        }
    }
}
