using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;

namespace Tellma.Api
{
    public interface IUserSettingsCache
    {
        Task<Versioned<UserSettingsForClient>> GetUserSettings(int userId, int tenantId, string version, CancellationToken cancellation = default);
    }
}