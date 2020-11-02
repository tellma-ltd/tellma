using Azure.Storage.Blobs;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Services.BlobStorage
{
    /// <summary>
    /// Implementation of <see cref="IBlobService"/> that stores blobs in an Azure blob storage,
    /// typically used in cloud installations to save on the storage cost of Azure SQL Server
    /// </summary>
    public class AzureBlobStorageService : IBlobService
    {
        private readonly AzureBlobStorageOptions _config;
        private readonly IInstrumentationService _instrumentation;

        public AzureBlobStorageService(AzureBlobStorageOptions config, IInstrumentationService instrumentation)
        {
            _config = config;
            _instrumentation = instrumentation;
        }

        /// <summary>
        /// Saves the byte array to the blob storage under the provided unique name, using the credentials in the configuration providers
        /// </summary>
        public async Task SaveBlobsAsync(IEnumerable<(string blobName, byte[] content)> blobs)
        {
            using var _ = _instrumentation.Block("SaveBlobsAsync");

            // Open a container client and get a reference to the single container
            BlobContainerClient containerClient = new BlobContainerClient(_config.ConnectionString, _config.ContainerName);
            await containerClient.CreateIfNotExistsAsync(); // The very first attachment

            foreach (var (blobName, content) in blobs)
            {
                // Create a blob client for the provided blob name
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                // Upload the byte array after turning it into a memory stream
                using var memStream = new MemoryStream(content);
                await blobClient.UploadAsync(memStream);
            }
        }

        /// <summary>
        /// Retrieves the byte array with the specified name from the blob storage, using the credentials in the configuration providers
        /// </summary>
        public async Task<byte[]> LoadBlob(string blobName, CancellationToken cancellation)
        {
            using var _ = _instrumentation.Block("LoadBlob");

            // Open a container client and get a reference to the single container
            BlobContainerClient containerClient = new BlobContainerClient(_config.ConnectionString, _config.ContainerName);
            if (!await containerClient.ExistsAsync(cancellation))
            {
                throw new BlobNotFoundException(blobName);
            }

            // Create a blob client for the provided blob name
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            if (!await blobClient.ExistsAsync(cancellation))
            {
                throw new BlobNotFoundException(blobName);
            }

            // Download and return as a byte array
            using MemoryStream stream = new MemoryStream();
            await blobClient.DownloadToAsync(stream, cancellation);
            
            byte[] result = stream.ToArray();
            return result;
        }

        /// <summary>
        /// Deletes the byte array with the specified name from the blob storage, using the credentials in the configuration providers
        /// </summary>
        public async Task DeleteBlobsAsync(IEnumerable<string> blobNames)
        {
            using var _ = _instrumentation.Block("DeleteBlobsAsync");

            // Open a container client and get a reference to the single container
            BlobContainerClient containerClient = new BlobContainerClient(_config.ConnectionString, _config.ContainerName);
            if (!await containerClient.ExistsAsync())
            {
                return;
            }

            // Delete the blobs one by one
            foreach (var blobName in blobNames)
            {
                BlobClient blobClient = containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync();
            }
        }
    }
}
