namespace Tellma.Services.Sharding
{
    public interface IShardResolver
    {
        string GetConnectionString(int? tenantId = null);
    }
}
