using System.Collections.Generic;

namespace Tellma.Controllers.Dto
{
    /// <summary>
    /// Almost all API handlers that return data will expect arguments that contain "select"
    /// and "expand" to specify what should be returned. All such argument DTOs inherit from
    /// this base class
    /// </summary>
    public class SelectExpandArguments
    {
        /// <summary>
        /// Equivalent to linq's "Select", determines which properties of the principal entities
        /// or of the expanded related entities to return in the result. If left empty then all
        /// properties of the principal entity and expanded entities are returned.
        /// Note: The select argument may be or may contain special "shorthands" that are understood by
        /// the controller and expanded into a proper select, useful in scenarios where the required
        /// select is enormous and unweildly yet very common
        /// </summary>
        public string Select { get; set; }

        /// <summary>
        /// Equivalent to linq's "Include", determines which related entities to include in 
        /// the result. If left empty then do not include any related entities
        /// </summary>
        public string Expand { get; set; }
    }
}
