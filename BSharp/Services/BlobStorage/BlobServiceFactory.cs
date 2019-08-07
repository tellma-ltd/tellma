using BSharp.Data;
using BSharp.Services.MultiTenancy;
using Microsoft.Extensions.Options;

namespace BSharp.Services.BlobStorage
{
    public class BlobServiceFactory : IBlobServiceFactory
    {
        private readonly BlobServiceOptions _config;
        private readonly ApplicationContext _db;
        private readonly ITenantIdAccessor _tenantIdProvider;

        public BlobServiceFactory(IOptions<BlobServiceOptions> options, ApplicationContext db, ITenantIdAccessor tenantIdProvider)
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
