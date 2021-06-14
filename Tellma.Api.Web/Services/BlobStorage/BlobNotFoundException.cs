using System;

namespace Tellma.Services.BlobStorage
{
    public class BlobNotFoundException : Exception
    {
        public BlobNotFoundException(string blobName)
        {
            BlobName = blobName;
        }

        public string BlobName { get; }
    }
}
