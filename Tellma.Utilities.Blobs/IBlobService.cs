using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Utilities.Blobs
{
    /// <summary>
    /// Interface used to store and retrieve blobs (images, attachments, etc..).
    /// The implementation will depend on the hosting environment.
    /// </summary>
    public interface IBlobService
    {
        /// <summary>
        /// Saves the given blobs under the provided names.
        /// </summary>
        /// <param name="tenantId">The tenant Id to which the blobs belong.</param>
        /// <param name="blobs">Collection of blobs, each blob with its unique identifying name.</param>
        Task SaveBlobsAsync(int tenantId, IEnumerable<(string blobName, byte[] content)> blobs);

        /// <summary>
        /// Retrieves the blob specified by the given name.
        /// </summary>
        /// <param name="tenantId">The tenant Id to which the blobs belong.</param>
        /// <param name="blobName">The name of the blob to load.</param>
        Task<byte[]> LoadBlobAsync(int tenantId, string blobName, CancellationToken cancellation);

        /// <summary>
        /// Deletes the blobs specified by the given names.
        /// </summary>
        /// <param name="tenantId">The tenant Id to which the blobs belong.</param>
        /// <param name="blobNames">Names of the blobs to delete.</param>
        Task DeleteBlobsAsync(int tenantId, IEnumerable<string> blobNames);
    }
}
