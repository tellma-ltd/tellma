using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;

namespace Tellma.Api.Behaviors
{
    public class IdentityServiceBehavior : IServiceBehavior
    {
        private readonly AdminServiceBehavior _adminBehavior;

        public IdentityServiceBehavior(AdminServiceBehavior adminBehavior)
        {
            _adminBehavior = adminBehavior;
        }

        public async Task<int> OnInitialize(CancellationToken cancellation)
        {
            return await _adminBehavior.OnInitialize(cancellation);
        }
    }
}
