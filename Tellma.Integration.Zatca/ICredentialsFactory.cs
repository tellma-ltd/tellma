namespace Tellma.Integration.Zatca
{
    public interface ICredentialsFactory
    {
        public Task<(string username, string password)> GetCredentials(CancellationToken cancellation);
    }

}