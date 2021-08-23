using IdentityServer4.Models;
using System.Collections.Generic;

namespace Tellma.Services.EmbeddedIdentityServer
{
    /// <summary>
    /// Implementations retrieve all the built-in clients used by human users.
    /// </summary>
    public interface IUserClientsProvider
    {
        IEnumerable<Client> UserClients();
    }
}
