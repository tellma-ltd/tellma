using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;

namespace Tellma.Api
{
    public interface IPermissionsCache
    {
        Task<Versioned<PermissionsForClient>> GetPermissionss(int userId, int tenantId, string version, CancellationToken cancellation = default);
    }
}