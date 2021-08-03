using System.Collections.Generic;
using Tellma.Repository.Common;

namespace Tellma.Repository.Application
{
    /// <summary>
    /// Contains errors and documentIds.
    /// </summary>
    public class SignResult : OperationResult
    {
        public SignResult(IEnumerable<ValidationError> errors, IEnumerable<int> documentIds) : base(errors)
        {
            DocumentIds = documentIds;
        }

        public IEnumerable<int> DocumentIds { get; }
    }
}
