using BSharp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.BlobStorage
{
    public class SqlTableBlobService : IBlobService
    {
        private readonly ApplicationRepository _repo;

        public SqlTableBlobService(ApplicationRepository repo)
        {
            _repo = repo;
        }

        public async Task DeleteBlobsAsync(IEnumerable<string> blobNames)
        {
            // Basic check
            if (blobNames is null)
            {
                throw new ArgumentNullException(nameof(blobNames));
            }

            await _repo.Blobs__Delete(blobNames);
        }

        public async Task<byte[]> LoadBlob(string blobName)
        {
            // Basic check
            if (blobName is null)
            {
                throw new ArgumentNullException(nameof(blobName));
            }

            // Retrieve the blob
            var blobContent = await _repo.Blobs__Get(blobName);
            if (blobContent == null)
            {
                throw new BlobNotFoundException(blobName);
            }

            return blobContent;
        }

        public async Task SaveBlobsAsync(IEnumerable<(string blobName, byte[] content)> blobs)
        {
            // Basic checks
            if (blobs is null)
            {
                throw new ArgumentNullException(nameof(blobs));
            }

            if (blobs.Any(e => e.content is null || e.blobName is null))
            {
                throw new Exception("At least one content or one blobName is null");
            }

            // TODO: Optimize for bulk save
            foreach (var (name, blob) in blobs)
            {
                await _repo.Blobs__Save(name, blob);
            }
        }
    }
}
