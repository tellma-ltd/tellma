using BSharp.Entities.ModelMetadata;
using System.Collections.Generic;

namespace BSharp.Data
{
    /// <summary>
    /// A structure that stores all definitions of a particular database
    /// </summary>
    public class DatabaseModelMetadata
    {
        /// <summary>
        /// Mapping from document type to document definition
        /// </summary>
        public Dictionary<string, DocumentModelMetadata> Documents { get; set; }

        /// <summary>
        /// Mapping from line type to line definition
        /// </summary>
        public Dictionary<string, LineModelMetadata> Lines { get; set; }

        /// <summary>
        /// Mapping from resource type to resource definition
        /// </summary>
        public Dictionary<string, ResourceModelMetadata> Resources { get; set; }

        /// <summary>
        /// The version of this cached item
        /// </summary>
        public string Version { get; set; }
    }
}
