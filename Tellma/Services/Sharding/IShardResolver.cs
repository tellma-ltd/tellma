using System.Threading.Tasks;
using System.Threading;

namespace Tellma.Services.Sharding
{
    public interface IShardResolver
    {
        Task<string> GetConnectionString(int? tenantId = null, CancellationToken cancellation = default);
    }
}
