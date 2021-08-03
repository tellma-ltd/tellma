using System.Collections.Generic;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Contains the result of validating and executing a delete database operation on an entity that has images.
    /// <para/>
    /// Note: This is specific to entities with integer Ids.
    /// </summary>
    public class DeleteWithImagesResult : DeleteResult
    {
        public DeleteWithImagesResult(IEnumerable<ValidationError> errors, List<string> deletedImageIds) : base(errors)
        {
            DeletedImageIds = deletedImageIds ?? new List<string>();
        }

        /// <summary>
        /// The Ids of the deleted images so they can be deleted from the blob storage.
        /// </summary>
        public List<string> DeletedImageIds { get; }
    }
}

