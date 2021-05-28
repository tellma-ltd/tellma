using System.Collections.Generic;
using Tellma.Utilities.Common;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Base class for all constant expressions such as 3 or 'Foo'.
    /// </summary>
    public abstract class TemplexConstant : TemplexBase
    {
        public override IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            // Constants don't need any select
            return AsyncUtil.Empty<Path>();
        }

        public override IAsyncEnumerable<Path> ComputePaths(EvaluationContext ctx)
        {
            // Constants aren't based on any paths
            return AsyncUtil.Empty<Path>();
        }
    }
}
