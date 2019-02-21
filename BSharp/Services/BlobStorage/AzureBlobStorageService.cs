using AutoMapper.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BSharp.Services.BlobStorage
{
    /// <summary>
    /// Contains a bunch of helper methods for accessing the Azure blob storage, storing attachments in 
    /// tables is very costly on SQL Azure so they are stored in the blob storage instead
    /// </summary>
    public class AzureBlobStorageService : IBlobService
    {
        private readonly string _notFoundError = "Sorry, the contents of this blob {0} were not found";
        private AzureBlobStorageConfiguration _config;

        public AzureBlobStorageService(AzureBlobStorageConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Saves the byte array to the blob storage under the given unique name provided, using the credentials in web.config file
        /// </summary>
        /// <param name="blobName">The name under which to store the blob, which is typically a GUID</param>
        /// <param name="content"></param>
        public async Task SaveBlobs(IEnumerable<(string blobName, byte[] content)> blobs)
        {
            foreach (var (blobName, content) in blobs)
            {
                // Get the cloud storage account from the settings
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_config.ConnectionString);

                // Open a cloud blob client and get a reference to the single container
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(_config.ContainerName);
                await container.CreateIfNotExistsAsync(); // The very first attachment

                // Create the block blob with the provided name and byte array
                CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
                await blob.UploadFromByteArrayAsync(content, 0, content.Length);
            }
        }

        /// <summary>
        /// Retrieves the byte array with the specified name from the blob storage, using the credentials in the configuration file
        /// </summary>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public async Task<byte[]> LoadBlob(string blobName)
        {
            // Get the cloud storage account from the settings
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_config.ConnectionString);

            // Open a cloud blob client and get a reference to the single container
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(_config.ContainerName);
            if (!await container.ExistsAsync())
                throw new Exception(string.Format(_notFoundError, blobName));

            // Get a reference to the block blob
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            if (!await blob.ExistsAsync())
                throw new Exception(string.Format(_notFoundError, blobName));

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
        /// Deletes the byte array with the specified name from the blob storage, using the credentials in web config file
        /// </summary>
        /// <param name="blobName"></param>
        public async Task DeleteBlobs(IEnumerable<string> blobNames)
        {
            foreach (var blobName in blobNames)
            {
                // Get the cloud storage account from the settings
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_config.ConnectionString);

                // Open a cloud blob client and get a reference to the single container
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(_config.ContainerName);
                if (!await container.ExistsAsync())
                    throw new Exception(string.Format(_notFoundError, blobName));

                // Get a reference to the block blob
                CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
                await blob.DeleteIfExistsAsync();
            }
        }
    }
}
