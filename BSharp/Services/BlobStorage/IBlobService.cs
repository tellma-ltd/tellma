using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.BlobStorage
{
    public interface IBlobService
    {
        Task SaveBlobs(IEnumerable<(string blobName, byte[] content)> blobs);

        Task<byte[]> LoadBlob(string blobName);

        Task DeleteBlobs(IEnumerable<string> blobNames);
    }
}
