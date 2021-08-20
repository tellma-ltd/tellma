using System.Collections.Generic;
using System.Linq;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Contains the result of validating and executing a generic database [api] operation.
    /// </summary>
    public class OperationOutput
    {
        public OperationOutput(IEnumerable<ValidationError> errors)
        {
            Errors = errors ?? new List<ValidationError>();
        }

        /// <summary>
        /// List of errors returned by the validation step of a database operation. 
        /// If there at least one error then the database operation did not execute.
        /// </summary>
        public IEnumerable<ValidationError> Errors { get; }

        /// <summary>
        /// Returns true if there is at least one error in <see cref="Errors"/>.
        /// </summary>
        public bool IsError => Errors.Any();
    }
}

