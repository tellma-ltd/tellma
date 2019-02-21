using BSharp.Data;
using BSharp.Services.MultiTenancy;
using Microsoft.Extensions.Options;

namespace BSharp.Services.BlobStorage
{
    public class BlobServiceFactory : IBlobServiceFactory
    {
        private readonly BlobServiceConfiguration _config;
        private readonly ApplicationContext _db;
        private readonly ITenantIdProvider _tenantIdProvider;

        public BlobServiceFactory(IOptions<BlobServiceConfiguration> options, ApplicationContext db, ITenantIdProvider tenantIdProvider)
        {
            _config = options.Value;
            _db = db;
            _tenantIdProvider = tenantIdProvider;
        }

        public IBlobService Create()
        {
            if (!string.IsNullOrWhiteSpace(_config.AzureBlobStorage.ConnectionString))
            {
                return new AzureBlobStorageService(_config.AzureBlobStorage);
            }
            else
            {
                return new SqlTableBlobService(_db, _tenantIdProvider);
            }
        }
    }
}
