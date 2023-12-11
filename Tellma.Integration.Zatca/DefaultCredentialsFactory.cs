namespace Tellma.Integration.Zatca
{
    internal class DefaultCredentialsFactory : ICredentialsFactory
    {
        public Task<(string username, string password)> GetCredentials(CancellationToken cancellation)
        {
            throw new NotImplementedException(); // TODO: Implement for production
        }
    }

}