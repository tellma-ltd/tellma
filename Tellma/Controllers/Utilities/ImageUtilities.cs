using Tellma.Data;
using Tellma.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tellma.Controllers.Utilities
{
    public static class ImageUtilities
    {
        /// <summary>
        /// Returns all new images in the saved entities with their blob names, after standardizing their size and format.
        /// Meanwhile for each entity with a new image the value of <see cref="EntityMetadata.FileId"/> on that entity is
        /// set to the file name (before applying the blob name function)
        /// </summary>
        public static IEnumerable<(string blobName, byte[] blobBytes)> ExtractImages<TEntity>(List<TEntity> entities, Func<string, string> blobNameFunc) where TEntity : Entity, IEntityWithImage
        {
            // Get new image Ids and bytes that should be added to blob storage
            foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
            {
                byte[] imageBytes = entity.Image;
                if (imageBytes != null)
                {
                    if (imageBytes.Length == 0) // This means delete the image
                    {
                        if (entity.Id != 0)
                        {
                            entity.EntityMetadata.FileId = null;
                        }
                    }
                    else
                    {
                        // Specify that ImageId should be set to a new GUID
                        string imageId = Guid.NewGuid().ToString();
                        entity.EntityMetadata.FileId = imageId;

                        // Below we process the new image bytes
                        // We make the image smaller and turn it into JPEG
                        using (var image = Image.Load(imageBytes))
                        {
                            // Resize to 128x128px
                            image.Mutate(c => c.Resize(new ResizeOptions
                            {
                                // 'Max' mode maintains the aspect ratio and keeps the entire image
                                Mode = ResizeMode.Max,
                                Size = new Size(128),
                                Position = AnchorPositionMode.Center
                            }));

                            // Some image formats like PNG support transparent regions
                            // These regions will turn black in JPEG format unless we do this
                            image.Mutate(c => c.BackgroundColor(new Rgba32(255, 255, 255))); ;

                            // Save as JPEG
                            var memoryStream = new MemoryStream();
                            image.SaveAsJpeg(memoryStream);
                            imageBytes = memoryStream.ToArray();

                            // Note: JPEG is the format of choice for photography.
                            // It provides better quality at a lower size for real life photographs
                            // which is what most of these pictures are expected to be
                        }

                        // Add it to blobs to create
                        string blobName = blobNameFunc(imageId);
                        yield return (blobName, imageBytes);
                    }
                }
            }
        }
    }
}
