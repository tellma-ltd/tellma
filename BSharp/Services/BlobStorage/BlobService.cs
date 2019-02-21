using System.Collections.Generic;
using System.Threading.Tasks;

namespace BSharp.Services.BlobStorage
{
    public class BlobService : IBlobService
    {
        private readonly IBlobService _blobService;

        public BlobService(IBlobServiceFactory factory)
        {
            _blobService = factory.Create();
        }

        public async Task DeleteBlobs(IEnumerable<string> blobNames)
        {
            await _blobService.DeleteBlobs(blobNames);
        }

        public async Task<byte[]> LoadBlob(string blobName)
        {
            return await _blobService.LoadBlob(blobName);
        }

        public async Task SaveBlobs(IEnumerable<(string blobName, byte[] content)> blobs)
        {
            await _blobService.SaveBlobs(blobs);
        }
    }
}
