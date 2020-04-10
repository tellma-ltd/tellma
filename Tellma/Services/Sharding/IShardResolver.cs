using System.Threading.Tasks;

namespace Tellma.Services.Sharding
{
    public interface IShardResolver
    {
        Task<string> GetConnectionString(int? tenantId = null);
    }
}
