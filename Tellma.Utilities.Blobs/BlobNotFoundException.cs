using System;

namespace Tellma.Utilities.Blobs
{
    public class BlobNotFoundException : Exception
    {
        public BlobNotFoundException(int tenantId, string blobName)
        {
            BlobName = blobName;
            TenantId = tenantId;
        }

        public int TenantId { get; }
        public string BlobName { get; }
    }
}
