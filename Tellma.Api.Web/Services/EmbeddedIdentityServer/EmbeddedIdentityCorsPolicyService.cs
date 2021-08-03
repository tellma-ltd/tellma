using IdentityServer4.Services;
using System.Threading.Tasks;
using Tellma.Services.ClientProxy;
using Tellma.Utilities.Common;

namespace Tellma.Services.EmbeddedIdentityServer
{
    public class EmbeddedIdentityCorsPolicyService : ICorsPolicyService
    {
        private readonly ClientAppAddressResolver _resolver;

        public EmbeddedIdentityCorsPolicyService(ClientAppAddressResolver resolver)
        {
            _resolver = resolver;
        }

        public Task<bool> IsOriginAllowedAsync(string origin)
        {
            var webClientOrigin = _resolver.Resolve().WithoutTrailingSlash();
            return Task.FromResult(origin == webClientOrigin);
        }
    }
}
