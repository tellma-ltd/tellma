using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Tellma.Utilities.Blobs;

namespace Tellma.Repository.Application
{
    /// <summary>
    /// Implementation of <see cref="IBlobService"/> that stores blobs in the application database.
    /// </summary>
    public class SqlBlobService : IBlobService
    {
        private readonly IApplicationRepositoryFactory _factory;

        public SqlBlobService(IApplicationRepositoryFactory factory)
        {
            _factory = factory;
        }

        public async Task DeleteBlobsAsync(int tenantId, IEnumerable<string> blobNames)
        {
            // Basic check
            if (blobNames is null)
            {
                throw new ArgumentNullException(nameof(blobNames));
            }

            var repo = _factory.GetRepository(tenantId);
            await repo.Blobs__Delete(blobNames); // Already bulk
        }

        public async Task<byte[]> LoadBlobAsync(int tenantId, string blobName, CancellationToken cancellation)
        {
            // Basic check
            if (blobName is null)
            {
                throw new ArgumentNullException(nameof(blobName));
            }

            // Retrieve the blob
            var repo = _factory.GetRepository(tenantId);
            var blobContent = await repo.Blobs__Get(blobName, cancellation);
            if (blobContent == null)
            {
                throw new BlobNotFoundException(tenantId, blobName);
            }

            return blobContent;
        }

        public async Task SaveBlobsAsync(int tenantId, IEnumerable<(string blobName, byte[] content)> blobs)
        {
            // Basic checks
            if (blobs is null)
            {
                throw new ArgumentNullException(nameof(blobs));
            }

            if (blobs.Any(e => e.content is null || e.blobName is null))
            {
                throw new InvalidOperationException("At least one content or one blobName is null.");
            }

            var repo = _factory.GetRepository(tenantId);
            await Task.WhenAll(blobs.Select(async (blob) =>
            {
                var (name, content) = blob;
                await repo.Blobs__Save(name, content);
            }));
        }
    }
}
