using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Utilities.Blobs
{
    /// <summary>
    /// Routes the requests to the available <see cref="IBlobService"/> implementation.
    /// </summary>
    public class BlobService : IBlobService
    {
        private readonly IBlobService _blobService;

        public BlobService(IBlobServiceFactory factory)
            => _blobService = factory.Create();

        public async Task DeleteBlobsAsync(int tenantId, IEnumerable<string> blobNames)
            => await _blobService.DeleteBlobsAsync(tenantId, blobNames);

        public async Task<byte[]> LoadBlob(int tenantId, string blobName, CancellationToken cancellation)
            => await _blobService.LoadBlob(tenantId, blobName, cancellation);

        public async Task SaveBlobsAsync(int tenantId, IEnumerable<(string blobName, byte[] content)> blobs)
            => await _blobService.SaveBlobsAsync(tenantId, blobs);
    }
}
