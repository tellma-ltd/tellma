using IdentityServer4.Models;
using IdentityServer4.Stores;
using System.Threading.Tasks;

namespace Tellma.Services.EmbeddedIdentityServer
{
    /// <summary>
    /// Implemention of <see cref="IClientStore"/> that retrieves a client through 
    /// the registered implementation of <see cref="IClientFinder"/>.
    /// </summary>
    public class ClientStore : IClientStore
    {
        private readonly IClientFinder _finder;

        public ClientStore(IClientFinder finder)
        {
            _finder = finder;
        }

        public async Task<Client> FindClientByIdAsync(string clientId) => 
            await _finder.FindClientByIdAsync(clientId);
    }
}
