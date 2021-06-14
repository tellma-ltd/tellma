using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Tellma.Services.Instrumentation
{
    /// <summary>
    /// Represents the measured time of a named section of code
    /// </summary>
    public class CodeBlockInstrumentation
    {
        /// <summary>
        /// For fast retrieval
        /// </summary>
        private readonly ConcurrentDictionary<string, CodeBlockInstrumentation> _dic = new ConcurrentDictionary<string, CodeBlockInstrumentation>();

        /// <summary>
        ///  Ordered
        /// </summary>
        private List<CodeBlockInstrumentation> _breakdown = null;

        /// <summary>
        /// The total time this section took. Kept short for smaller response size
        /// </summary>
        [JsonProperty(Order = 1)]
        public long T { get; set; } = 0;

        /// <summary>
        /// The name of this section. Kept short for smaller response size
        /// </summary>
        [JsonProperty(Order = 2)]
        public string N { get; set; }

        /// <summary>
        /// The instrumented blocks that took at least 1 millisecond. Name kept short for smaller response size
        /// </summary>
        [JsonProperty(Order = 3)]
        public IEnumerable<CodeBlockInstrumentation> B => _breakdown?.Where(e => e.T != 0);

        /// <summary>
        /// Adds a step with the specified name
        /// </summary>
        /// <param name="name">The name of the step</param>
        public CodeBlockInstrumentation AddSubBlock(string name)
        {
            return _dic.GetOrAdd(name, (name) =>
            {
                var instrumentation = new CodeBlockInstrumentation { N = name };

                // Add to the breakdown
                _breakdown ??= new List<CodeBlockInstrumentation>();
                _breakdown.Add(instrumentation);

                return instrumentation;
            });
        }
    }
}
