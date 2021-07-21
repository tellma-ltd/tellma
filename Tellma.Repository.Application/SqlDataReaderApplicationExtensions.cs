using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Repository.Common;

namespace Tellma.Repository.Application
{
    public static class SqlDataReaderApplicationExtensions
    {
        /// <summary>
        /// First loads the <see cref="ValidationError"/>s, if none are returned it moves
        /// to the next result set and loads the ids of deleted images, then if returnIds 
        /// is true moves to the next result set and loads the entity ids sorted by index. 
        /// Returns the errors, the ids, and images ids in a <see cref="SaveWithImagesResult"/> object.
        /// </summary>
        /// <param name="returnIds">Whether or not to return the Ids.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        public static async Task<SaveWithImagesResult> LoadSaveWithImagesResult(this SqlDataReader reader, bool returnIds, CancellationToken cancellation = default)
        {
            // (1) Load the errors
            var errors = await reader.LoadErrors(cancellation);

            // (2) Load the deleted image ids
            var deletedImageIds = new List<string>();
            if (!errors.Any())
            {
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    deletedImageIds.Add(reader.String(0));
                }
            }

            // (3) If no errors => load the Ids
            List<int> ids = null;
            if (returnIds && !errors.Any())
            {
                await reader.NextResultAsync(cancellation);
                ids = await reader.LoadIds(cancellation);
            }

            // (4) Return the result
            return new SaveWithImagesResult(errors, ids, deletedImageIds);
        }

        /// <summary>
        /// First loads the <see cref="ValidationError"/>s, if none are returned it moves 
        /// to the next result set and loads the ids of deleted images. 
        /// Returns the errors and images ids in a <see cref="DeleteWithImagesResult"/> object.
        /// </summary>
        /// <param name="cancellation">The cancellation instruction.</param>
        public static async Task<DeleteWithImagesResult> LoadDeleteWithImagesResult(this SqlDataReader reader, CancellationToken cancellation = default)
        {
            // (1) Load the errors
            var errors = await reader.LoadErrors(cancellation);

            // (2) Load the deleted image ids
            var deletedImageIds = new List<string>();
            if (!errors.Any())
            {
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    deletedImageIds.Add(reader.String(0));
                }
            }

            // (3) Return the result
            return new DeleteWithImagesResult(errors, deletedImageIds);
        }

        /// <summary>
        /// First loads the <see cref="ValidationError"/>s, if none are returned and returnIds is true it moves
        /// to the next result set and loads the document ids. Returns both the errors and the ids in a <see cref="SaveResult"/> object.
        /// </summary>
        /// <param name="returnIds">Whether or not to return the document Ids.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        public static async Task<SignResult> LoadSignResult(this SqlDataReader reader, bool returnIds, CancellationToken cancellation = default)
        {
            // (1) Load the errors
            var errors = await reader.LoadErrors(cancellation);

            // (2) If no errors => load the Ids
            var documentIds = new List<int>();
            if (returnIds && !errors.Any())
            {
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    documentIds.Add(reader.GetInt32(0));
                }
            }

            // (3) Return the result
            return new SignResult(errors, documentIds);
        }
    }
}
