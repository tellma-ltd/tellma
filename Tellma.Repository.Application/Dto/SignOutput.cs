using System.Collections.Generic;
using Tellma.Repository.Common;

namespace Tellma.Repository.Application
{
    /// <summary>
    /// Contains errors and documentIds.
    /// </summary>
    public class SignOutput : OperationOutput
    {
        public SignOutput(IEnumerable<ValidationError> errors, IEnumerable<int> documentIds) : base(errors)
        {
            DocumentIds = documentIds;
        }

        public IEnumerable<int> DocumentIds { get; }
    }
}
