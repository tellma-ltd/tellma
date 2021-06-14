using Tellma.Data;
using Microsoft.Extensions.Options;

namespace Tellma.Services.BlobStorage
{
    /// <summary>
    /// Generated an appropriate implementation of <see cref="IBlobService"/> depending on the environment
    /// </summary>
    public class BlobServiceFactory : IBlobServiceFactory
    {
        private readonly BlobServiceOptions _config;
        private readonly ApplicationRepository _repo;
        private readonly IInstrumentationService _instrumentationService;

        public BlobServiceFactory(IOptions<BlobServiceOptions> options, ApplicationRepository repo, IInstrumentationService instrumentationService)
        {
            _config = options.Value;
            _repo = repo;
            _instrumentationService = instrumentationService;
        }

        /// <summary>
        /// Returns either a <see cref="AzureBlobStorageService"/> or a <see cref="SqlTableBlobService"/> depending on the environment
        /// </summary>
        public IBlobService Create()
        {
            if (!string.IsNullOrWhiteSpace(_config?.AzureBlobStorage?.ConnectionString))
            {
                return new AzureBlobStorageService(_config.AzureBlobStorage, _instrumentationService);
            }
            else
            {
                return new SqlTableBlobService(_repo);
            }
        }
    }
}
