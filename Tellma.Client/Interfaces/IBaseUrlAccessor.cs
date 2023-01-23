using System.Collections.Generic;

namespace Tellma.Client
{
    public interface IBaseUrlAccessor
    {
        IEnumerable<string> GetBaseUrlSteps();
    }
}
