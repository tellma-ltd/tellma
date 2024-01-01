namespace Tellma.Integration.Zatca
{
    internal class DefaultCredentialsFactory : ICredentialsFactory
    {
        public Task<(string username, string password)> GetCredentials(CancellationToken cancellation)
        {
            throw new NotImplementedException(); // ??? Figure out how this works for production
        }
    }

}