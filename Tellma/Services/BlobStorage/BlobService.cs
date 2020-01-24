using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tellma.Services.BlobStorage
{
    public class BlobService : IBlobService
    {
        private readonly IBlobService _blobService;

        public BlobService(IBlobServiceFactory factory)
        {
            _blobService = factory.Create();
        }

        public async Task DeleteBlobsAsync(IEnumerable<string> blobNames)
        {
            await _blobService.DeleteBlobsAsync(blobNames);
        }

        public async Task<byte[]> LoadBlob(string blobName)
        {
            return await _blobService.LoadBlob(blobName);
        }

        public async Task SaveBlobsAsync(IEnumerable<(string blobName, byte[] content)> blobs)
        {
            await _blobService.SaveBlobsAsync(blobs);
        }
    }
}
