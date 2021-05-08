using System.Threading.Tasks;

namespace Tellma.Utilities.Sharding
{
    public interface ICachingShardResolver
    {
        Task<string> GetConnectionString(int databaseId);
    }
}
