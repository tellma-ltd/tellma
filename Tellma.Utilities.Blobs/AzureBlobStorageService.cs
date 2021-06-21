using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Utilities.Blobs
{
    /// <summary>
    /// Implementation of <see cref="IBlobService"/> that stores blobs in an Azure blob storage,
    /// typically used in cloud installations to save on the storage cost of Azure SQL Server.
    /// </summary>
    public class AzureBlobStorageService : IBlobService
    {
        private readonly AzureBlobStorageOptions _config;

        public AzureBlobStorageService(AzureBlobStorageOptions config)
        {
            _config = config;
        }

        /// <summary>
        /// Saves the byte array to the blob storage under the provided unique name, 
        /// using the credentials in the configuration providers.
        /// </summary>
        public async Task SaveBlobsAsync(int tenantId, IEnumerable<(string blobName, byte[] content)> blobs)
        {
            // Get the container client
            BlobContainerClient containerClient = await ContainerClient(cancellation: default);

            // Create the blobs in parallel
            await Task.WhenAll(blobs.Select(async (blob) =>
            {
                // Deconstruct the blob name and content
                var (blobName, content) = blob;

                // Create a blob client for the provided blob name
                string qualifiedName = QualifiedBlobName(tenantId, blobName);
                BlobClient blobClient = containerClient.GetBlobClient(qualifiedName);

                // Upload the byte array after turning it into a memory stream
                using var memStream = new MemoryStream(content);
                await blobClient.UploadAsync(memStream);
            }));
        }

        /// <summary>
        /// Retrieves the byte array with the specified name from the blob storage, 
        /// using the credentials in the configuration providers.
        /// </summary>
        public async Task<byte[]> LoadBlob(int tenantId, string blobName, CancellationToken cancellation)
        {
            // Open a container client and get a reference to the single container
            BlobContainerClient containerClient = await ContainerClient(cancellation: default);

            // Create a blob client for the provided blob name
            string qualifiedName = QualifiedBlobName(tenantId, blobName);
            BlobClient blobClient = containerClient.GetBlobClient(qualifiedName);
            if (!await blobClient.ExistsAsync(cancellation))
            {
                throw new BlobNotFoundException(tenantId, blobName);
            }

            // Download and return as a byte array
            using var stream = new MemoryStream();
            await blobClient.DownloadToAsync(stream, cancellation);

            byte[] result = stream.ToArray();
            return result;
        }

        /// <summary>
        /// Deletes the byte array with the specified name from the blob storage, 
        /// using the credentials in the configuration providers.
        /// </summary>
        public async Task DeleteBlobsAsync(int tenantId, IEnumerable<string> blobNames)
        {
            // Open a container client and get a reference to the single container
            BlobContainerClient containerClient = await ContainerClient(cancellation: default);

            // Delete the blobs in parallel
            await Task.WhenAll(blobNames.Select(async (blobName) =>
            {
                string qualifiedName = QualifiedBlobName(tenantId, blobName);
                BlobClient blobClient = containerClient.GetBlobClient(qualifiedName);
                await blobClient.DeleteIfExistsAsync();
            }));
        }

        #region Helpers

        private static bool _isInit = false;
        private static readonly SemaphoreSlim _initSemaphore = new(initialCount: 1);

        /// <summary>
        /// Creates a container client to the container specified in <see cref="AzureBlobStorageOptions"/>.
        /// If this is the first time, it creates the container on the blob storage if it doesn't exist.
        /// </summary>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>The <see cref="BlobContainerClient"/> to the blob storage specified in the configurations.</returns>
        private async Task<BlobContainerClient> ContainerClient(CancellationToken cancellation)
        {
            // Get the connection string
            var connString = _config.ConnectionString;
            if (string.IsNullOrWhiteSpace(connString))
            {
                throw new InvalidOperationException("No ConnectionString to an Azure Blob Storage container was not provided.");
            }

            // Get the container name
            var containerName = _config.ContainerName;
            if (string.IsNullOrWhiteSpace(containerName))
            {
                containerName = "default_container";
            }

            // Open a container client and get a reference to the single container
            var client = new BlobContainerClient(connString, containerName);
            if (!_isInit)
            {
                await _initSemaphore.WaitAsync(cancellation);
                try
                {
                    // A second OCD-check inside the semaphore block
                    if (!_isInit)
                    {
                        // On the very first call, we ensure that the container is created.
                        await client.CreateIfNotExistsAsync(cancellationToken: cancellation);
                        _isInit = true;
                    }
                }
                finally
                {
                    // Very important
                    _initSemaphore.Release();
                }
            }

            return client;
        }

        private static string QualifiedBlobName(int tenantId, string blobName) => $"{tenantId}/{blobName}";

        #endregion
    }
}
