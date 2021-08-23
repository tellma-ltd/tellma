namespace Tellma.Repository.Application
{
    public interface IApplicationRepositoryFactory
    {
        ApplicationRepository GetRepository(int tenantId);
    }
}
