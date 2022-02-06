using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;

namespace Tellma.Api.Behaviors
{
    public class NullServiceBehavior : IServiceBehavior
    {
        public Task<int> OnInitialize(IServiceContextAccessor contextAccessor, CancellationToken _) => Task.FromResult(0);
    }
}
