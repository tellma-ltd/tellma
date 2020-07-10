using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tellma.Services.Instrumentation
{
    /// <summary>
    /// Serves as the root of the report
    /// </summary>
    public class InstrumentationReport
    {
        /// <summary>
        /// Total overhead time spent on collecting the instrumentation
        /// </summary>
        [JsonProperty(Order = 3)]
        public long Overhead { get; set; }

        /// <summary>
        /// The total time of the request
        /// </summary>
        [JsonProperty(Order = 1)]
        public long Total { get; set; }

        /// <summary>
        /// The tree of measured code
        /// </summary>
        [JsonProperty(Order = 2)]
        public IEnumerable<CodeBlockInstrumentation> Breakdown { get; set; }

        /// <summary>
        /// The instrumentation of the middleware steps 
        /// (Given special treatment since each middleware calls the next middelware, and we want to 
        /// measure every middleware on its own without the part where it calls the next middleware)
        /// </summary>
        [JsonProperty(Order = 3)]
        public List<CodeBlockInstrumentation> Middleware { get; set; }
    }
}
