using System.Threading.Tasks;

namespace Tellma.Utilities.Sharding
{
    public interface IShardResolver
    {
        public Task<DatabaseInfo> Resolve(int databaseId);

        public Task<string> DefaultConnectionString();
    }
}
