using System.Collections.Generic;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Contains the result of validating and executing a delete database operation.
    /// </summary>
    public class DeleteOutput : OperationOutput
    {
        public DeleteOutput(IEnumerable<ValidationError> errors) : base(errors)
        {
        }
    }
}

