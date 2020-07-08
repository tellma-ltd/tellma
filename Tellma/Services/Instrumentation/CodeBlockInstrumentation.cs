using Newtonsoft.Json;
using System.Collections.Generic;

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
        private readonly Dictionary<string, CodeBlockInstrumentation> _dic = new Dictionary<string, CodeBlockInstrumentation>();

        /// <summary>
        ///  Ordered
        /// </summary>
        [JsonProperty(Order = 3)]
        public List<CodeBlockInstrumentation> Breakdown { get; set; }

        /// <summary>
        /// The name of this section
        /// </summary>
        [JsonProperty(Order = 2)]
        public string Name { get; set; }

        /// <summary>
        /// The total time this section took
        /// </summary>
        [JsonProperty(Order = 1)]
        public long Total { get; set; } = 0;

        /// <summary>
        /// Adds a step with the specified name
        /// </summary>
        /// <param name="name">The name of the step</param>
        public CodeBlockInstrumentation AddSubBlock(string name)
        {
            if (!_dic.TryGetValue(name, out CodeBlockInstrumentation instrumentation))
            {
                instrumentation = new CodeBlockInstrumentation { Name = name };
                _dic.Add(name, instrumentation);

                // Add to the breakdown
                Breakdown ??= new List<CodeBlockInstrumentation>();
                Breakdown.Add(instrumentation);
            }

            return instrumentation;
        }
    }
}
