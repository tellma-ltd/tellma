using IdentityServer4.Models;
using System.Threading.Tasks;

namespace Tellma.Services.EmbeddedIdentityServer
{
    /// <summary>
    /// Implementations retrieve a clients using a client Id;
    /// </summary>
    public interface IClientFinder
    {
        Task<Client> FindClientByIdAsync(string clientId);
    }
}
