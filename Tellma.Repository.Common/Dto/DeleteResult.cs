using System.Collections.Generic;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Contains the result of validating and executing a delete database operation.
    /// </summary>
    public class DeleteResult : OperationResult
    {
        public DeleteResult(IEnumerable<ValidationError> errors) : base(errors)
        {
        }
    }
}

