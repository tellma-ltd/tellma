namespace BSharp.Services.Sharding
{
    public interface IShardResolver
    {
        string GetConnectionString(int? tenantId = null);
    }
}
