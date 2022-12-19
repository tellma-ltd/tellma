namespace Tellma.Api.Behaviors
{
    public interface ITenantLogger
    {
        void Log(TenantLogEntry entry);
    }
}
