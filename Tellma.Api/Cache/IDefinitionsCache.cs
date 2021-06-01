using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;

namespace Tellma.Api
{
    public interface IDefinitionsCache
    {
        Task<Versioned<DefinitionsForClient>> GetDefinitions(int tenantId, string version, CancellationToken cancellation = default);
    }
}