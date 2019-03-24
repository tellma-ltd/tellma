using BSharp.Data;
using BSharp.Data.Model;
using BSharp.Services.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.BlobStorage
{
    public class SqlTableBlobService : IBlobService
    {
        private readonly string _notFoundError = "Sorry, the contents of this blob {0} were not found";
        private readonly ApplicationContext _db;
        private readonly ITenantIdProvider _tenantIdProvider;

        public SqlTableBlobService(ApplicationContext db, ITenantIdProvider tenantIdProvider)
        {
            _db = db;
            _tenantIdProvider = tenantIdProvider;
        }

        public async Task DeleteBlobs(IEnumerable<string> blobNames)
        {
            // Basic check
            if (blobNames == null)
            {
                throw new ArgumentNullException(nameof(blobNames));
            }

            var blobNamesString = string.Join(',', blobNames);
            int tenantId = _tenantIdProvider.GetTenantId().Value;
            await _db.Database.ExecuteSqlCommandAsync($"DELETE FROM [dbo].[Blobs] WHERE Id IN (SELECT VALUE FROM STRING_SPLIT({blobNamesString}, ',')) AND TenantId = {tenantId}");
        }

        public async Task<byte[]> LoadBlob(string blobName)
        {
            // Basic check
            if (blobName == null)
            {
                throw new ArgumentNullException(nameof(blobName));
            }

            // Retrieve the blob
            var blob = await _db.Blobs.FirstOrDefaultAsync(e => e.Id == blobName);
            if (blob == null)
            {
                throw new Exception(_notFoundError);
            }

            return blob.Content;
        }

        public async Task SaveBlobs(IEnumerable<(string blobName, byte[] content)> blobs)
        {
            // Basic checks
            if (blobs == null)
            {
                throw new ArgumentNullException(nameof(blobs));
            }

            if (blobs.Any(e => e.content == null || e.blobName == null))
            {
                throw new Exception("At least one content or one blobName is null");
            }

            // TODO: Optimize for bulk save
            var blobsForSave = blobs.Select(e => new Blob
            {
                Id = e.blobName,
                Content = e.content
            });

            // _db.Blobs.AddRange(blobsForSave);

            // Set tenant Id
            int tenantId = _tenantIdProvider.GetTenantId().Value;
            foreach(var blob in blobsForSave)
            {
                _db.Blobs.Add(blob);
                _db.Entry(blob).Property("TenantId").CurrentValue = tenantId;
            }

            await _db.SaveChangesAsync();
        }
    }
}
