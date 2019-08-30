using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BSharp.Services.BlobStorage
{
    /// <summary>
    /// Implementation of <see cref="IBlobService"/> that stores blobs in an Azure blob storage,
    /// typically used in cloud installations to save on the storage cost of Azure SQL Server
    /// </summary>
    public class AzureBlobStorageService : IBlobService
    {
        private readonly AzureBlobStorageOptions _config;

        public AzureBlobStorageService(AzureBlobStorageOptions config)
        {
            _config = config;
        }

        /// <summary>
        /// Saves the byte array to the blob storage under the provided unique name, using the credentials in the configuration providers
        /// </summary>
        public async Task SaveBlobsAsync(IEnumerable<(string blobName, byte[] content)> blobs)
        {
            // Get the cloud storage account from the settings
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_config.ConnectionString);

            // Open a cloud blob client and get a reference to the single container
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(_config.ContainerName);
            await container.CreateIfNotExistsAsync(); // The very first attachment

            foreach (var (blobName, content) in blobs)
            {
                // Create the block blob with the provided name and byte array
                CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
                await blob.UploadFromByteArrayAsync(content, 0, content.Length);
            }
        }

        /// <summary>
        /// Retrieves the byte array with the specified name from the blob storage, using the credentials in the configuration providers
        /// </summary>
        public async Task<byte[]> LoadBlob(string blobName)
        {
            // Get the cloud storage account from the settings
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_config.ConnectionString);

            // Open a cloud blob client and get a reference to the single container
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(_config.ContainerName);
            if (!await container.ExistsAsync())
            {
                throw new BlobNotFoundException(blobName);
            }

            // Get a reference to the block blob
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            if (!await blob.ExistsAsync())
            {
                throw new BlobNotFoundException(blobName);
            }

            // Download and return as a byte array
            byte[] result = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(stream);
                result = stream.ToArray();
            }

            return result;
        }

        /// <summary>
        /// Deletes the byte array with the specified name from the blob storage, using the credentials in the configuration providers
        /// </summary>
        public async Task DeleteBlobsAsync(IEnumerable<string> blobNames)
        {
            // Get the cloud storage account from the settings
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_config.ConnectionString);

            // Open a cloud blob client and get a reference to the single container
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(_config.ContainerName);

            foreach (var blobName in blobNames)
            {
                if (!await container.ExistsAsync())
                {
                    throw new BlobNotFoundException(blobName);
                }

                // Get a reference to the block blob
                CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
                await blob.DeleteIfExistsAsync();
            }
        }
    }
}
