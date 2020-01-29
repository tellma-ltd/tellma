using Tellma.Data;
using Tellma.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Controllers.Utilities
{
    public static class ImageUtilities
    {
        public static async Task<(List<string> BlobsToDelete, List<(string, byte[])> BlobsToSave, List<IndexedImageId> ImageIds)> ExtractImages<TEntity, TEntityForSave>(
            IRepository repo, List<TEntityForSave> entities, Func<string, string> blobName) where TEntityForSave : IEntityWithImageForSave where TEntity : Entity, IEntityWithImage
        {
            var blobsToDelete = new List<string>();
            var blobsToSave = new List<(string, byte[])>();
            var imageIds = new List<IndexedImageId>(); // For the repository

            var idsWithNewImages = entities
                .Where(e => e.Image != null && e.Id != 0)
                .Select(e => e.Id)
                .ToArray();

            if (idsWithNewImages.Any())
            {
                // Get old image Ids that should be deleted from Blob storage
                var dbEntitiesWithNewImages = await repo.Query<TEntity>()
                    .Select(nameof(IEntityWithImage.ImageId))
                    .Filter($"{nameof(IEntityWithImage.ImageId)} ne null")
                    .FilterByIds(idsWithNewImages)
                    .ToListAsync();

                blobsToDelete = dbEntitiesWithNewImages
                    .Select(e => blobName(e.ImageId))
                    .ToList();
            }

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
                            // Specify that ImageId should be set to NULL
                            imageIds.Add(new IndexedImageId
                            {
                                Index = index,
                                ImageId = null
                            });
                        }
                    }
                    else
                    {
                        // Specify that ImageId should be set to a new GUID
                        string imageId = Guid.NewGuid().ToString();
                        imageIds.Add(new IndexedImageId
                        {
                            Index = index,
                            ImageId = imageId
                        });

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

                            // Some image formats that support transparent regions
                            // these regions will turn black in JPEG format unless we do this
                            image.Mutate(c => c.BackgroundColor(Rgba32.White)); ;

                            // Save as JPEG
                            var memoryStream = new MemoryStream();
                            image.SaveAsJpeg(memoryStream);
                            imageBytes = memoryStream.ToArray();

                            // Note: JPEG is the format of choice for photography.
                            // It provides better quality at a lower size for photographs
                            // which is what most of these pictures are expected to be
                        }

                        // Add it to blobs to create
                        blobsToSave.Add((blobName(imageId), imageBytes));
                    }
                }
            }

            return (blobsToDelete, blobsToSave, imageIds);
        }
    }
}
