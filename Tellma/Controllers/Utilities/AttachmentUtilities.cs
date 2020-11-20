using System;
using System.Collections.Generic;
using Tellma.Entities;

namespace Tellma.Controllers.Utilities
{
    public static class AttachmentUtilities
    {
        /// <summary>
        /// Returns all new images in the saved entities with their blob names, after standardizing their size and format.
        /// Meanwhile for each entity with a new image the value of <see cref="EntityMetadata.FileId"/> on that entity is
        /// set to the file name (before applying the blob name function)
        /// </summary>
        public static IEnumerable<(string blobName, byte[] blobBytes)> ExtractAttachments<T>(List<T> entities, Func<T, IEnumerable<IAttachment>> attachmentsFunc, Func<string, string> blobNameFunc)
        {
            // The new attachments
            foreach (var entity in entities)
            {
                var attachments = attachmentsFunc(entity);
                if (attachments != null)
                {
                    foreach (var a in attachments)
                    {                    // New attachments
                        if (a.Id == 0)
                        {
                            // Add extras: file Id and size
                            byte[] fileBytes = a.File;
                            string fileId = Guid.NewGuid().ToString();

                            // Set it in the attachment metadata
                            a.EntityMetadata.FileId = fileId;
                            a.EntityMetadata.FileSize = fileBytes.LongLength;

                            // Add to _blobsToSave
                            string blobName = blobNameFunc(fileId);
                            yield return (blobName, fileBytes);
                        }
                    }
                }
            }
        }
    }
}
