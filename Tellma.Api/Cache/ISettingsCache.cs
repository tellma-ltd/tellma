using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;

namespace Tellma.Api
{
    public interface ISettingsCache
    {
        Task<Versioned<SettingsForClient>> GetSettings(int tenantId, string version, CancellationToken cancellation = default);
    }
}