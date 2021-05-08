namespace Tellma.Utilities.Sharding
{
    public interface ICachingShardResolver
    {
        string GetConnectionString(int databaseId);
    }
}
