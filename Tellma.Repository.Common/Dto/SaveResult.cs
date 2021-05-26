using System.Collections.Generic;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Contains the result of preprocessing, validating and executing a save database operation.
    /// <para/>
    /// Note: This is specific to entities with integer Ids.
    /// </summary>
    public class SaveResult : OperationResult
    {
        public SaveResult(IEnumerable<ValidationError> errors, IReadOnlyList<int> ids) : base(errors)
        {
            Ids = ids ?? new List<int>();
        }

        /// <summary>
        /// The Ids of the saved entities in the same order they came in.
        /// </summary>
        public IReadOnlyList<int> Ids { get; }
    }
}

