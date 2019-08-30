using System.Collections.Generic;
using System.Threading.Tasks;

namespace BSharp.Services.BlobStorage
{
    /// <summary>
    /// Interface used to store and retrieve blobs (images, attachments, etc..).
    /// The implementation will depend on the hosting environment.
    /// </summary>
    public interface IBlobService
    {
        /// <summary>
        /// Saves the given blobs under the provided names
        /// </summary>
        /// <param name="blobs">Collection of blobs, each blob with its unique identifying name</param>
        Task SaveBlobsAsync(IEnumerable<(string blobName, byte[] content)> blobs);

        /// <summary>
        /// Retrieves the has the given name
        /// </summary>
        /// <param name="blobName">The name of the blob to load</param>
        Task<byte[]> LoadBlob(string blobName);

        /// <summary>
        /// Deletes the blobs specified by the given names
        /// </summary>
        /// <param name="blobNames">Names of the blobs to delete</param>
        Task DeleteBlobsAsync(IEnumerable<string> blobNames);
    }
}
