using System.Threading.Tasks;
using Tellma.Api.Base;

namespace Tellma.Api.Behaviors
{
    public class NullServiceBehavior : IServiceBehavior
    {
        public Task<int> OnInitialize()
        {
            return Task.FromResult(0);
        }
    }
}
