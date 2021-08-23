using IdentityServer4.Models;
using IdentityServer4.Stores;
using System.Linq;
using System.Threading.Tasks;
using Tellma.Repository.Admin;
using Tellma.Services.Utilities;

namespace Tellma.Services.EmbeddedIdentityServer
{
    /// <summary>
    /// Implemention of <see cref="IClientStore"/> that retrieves a client through 
    /// the registered implementation of <see cref="IUserClientsProvider"/>.
    /// </summary>
    public class ClientStore : IClientStore
    {
        private readonly IUserClientsProvider _provider;
        private readonly AdminRepository _repo;

        public ClientStore(IUserClientsProvider provider, AdminRepository repo)
        {
            _provider = provider;
            _repo = repo;
        }

        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            if (clientId == null)
            {
                return null;
            }

            // First search through the human user clients
            var query = from c in _provider.UserClients()
                        where c.ClientId == clientId
                        select c;

            var client = query.SingleOrDefault();
            if (client == null)
            {
                // Then search through the machine-2-machine clients
                var (dbClientId, dbClientSecret) = await _repo.IdentityServerClients__FindByClientId(clientId);
                if (dbClientId == clientId) // Database logic may be case insensitive
                {
                    // This is a valid client that authenticates using the client credentials flow (Machine-2-Machine)
                    client = new Client
                    {
                        ClientId = dbClientId,
                        ClientSecrets =
                        {
                            new Secret(dbClientSecret.Sha256())
                        },
                        AllowedGrantTypes = GrantTypes.ClientCredentials,
                        AllowedScopes =
                        {
                            Constants.ApiResourceName
                        }
                    };
                }
            }

            return client;
        }
    }
}
