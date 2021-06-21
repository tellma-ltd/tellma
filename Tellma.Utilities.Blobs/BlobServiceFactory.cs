using Microsoft.Extensions.Options;
using Tellma.Repository.Application;

namespace Tellma.Utilities.Blobs
{
    /// <summary>
    /// Creates an implementation of <see cref="IBlobService"/> depeneding on the environment configuration. <br/>
    /// - First attempst to create a <see cref="AzureBlobStorageService"/> if the blob storage connection string is supplied. <br/>
    /// - Else creates a <see cref="SqlTableBlobService"/>.
    /// </summary>
    public class BlobServiceFactory : IBlobServiceFactory
    {
        private readonly BlobServiceOptions _config;
        private readonly IApplicationRepositoryFactory _repo;

        public BlobServiceFactory(IOptions<BlobServiceOptions> options, IApplicationRepositoryFactory repo)
        {
            _config = options.Value;
            _repo = repo;
        }

        /// <summary>
        /// Creates a implementation of <see cref="IBlobService"/> depeneding on the environment configuration. 
        /// </summary>
        public IBlobService Create()
        {
            if (!string.IsNullOrWhiteSpace(_config?.AzureBlobStorage?.ConnectionString))
            {
                return new AzureBlobStorageService(_config.AzureBlobStorage);
            }
            else
            {
                return new SqlTableBlobService(_repo);
            }
        }
    }
}
