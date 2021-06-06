using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;

namespace Tellma.Api.Behaviors
{
    public class NullServiceBehavior : IServiceBehavior
    {
        public Task<int> OnInitialize(CancellationToken _) => Task.FromResult(0);
    }
}
