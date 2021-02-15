using System.Collections.Generic;

namespace Tellma.Entities
{
    /// <summary>
    /// The datatype for the entity metadata that is attached to every <see cref="Entity"/>, 
    /// the metadata maps every property name in the DTO to whether it is loaded or restricted.
    /// Since this entity inherits from Dictionary, JSON.NET will not serialize and deserialize the properties therein
    /// </summary>
    public class EntityMetadata : Dictionary<string, FieldMetadata>
    {
        /// <summary>
        /// Returns true if the given property name is marked as loaded in this <see cref="EntityMetadata"/>
        /// </summary>
        /// <param name="propName">The name of the property to inspect</param>
        /// <returns>True if the given property name is marked as loaded in this <see cref="EntityMetadata"/>, false otherwise.</returns>
        public bool IsLoaded(string propName) => propName == "Id" || TryGetValue(propName, out FieldMetadata meta) && meta == FieldMetadata.Loaded;

        /// <summary>
        /// Used by import to map entities to the row they came from.
        /// Since the containing entity is a dictionary, JSON.NET will not serialize this value
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// Used by import to map properties to the column they came from.
        /// Since the containing entity is a dictionary, JSON.NET will not serialize this value
        /// </summary>
        public object MappingInfo { get; set; }

        /// <summary>
        /// Used by import to map properties to the column they came from.
        /// Since the containing entity is a dictionary, JSON.NET will not serialize this value
        /// </summary>
        public Entity BaseEntity { get; set; }

        /// <summary>
        /// Used by the flatten and trim logic to remember which entities have already been flattened and trimmed
        /// </summary>
        public bool FlattenedAndTrimmed { get; set; }

        /// <summary>
        /// Used to store any error message when parsing <see cref="ILocationEntityForSave.LocationJson"/>
        /// </summary>
        public string LocationJsonParseError { get; set; }

        #region Self Referencing

        /// <summary>
        /// Stores the code or name of the related entity, used by the import logic when
        /// an entity's user key may refer to another entity in the imported list (self referencing FKs)
        /// </summary>
        public (object userKeyValue, IEnumerable<EntityWithKey> matches)[] MatchPairs { get; set; }

        /// <summary>
        /// Returns true if matches were set at this index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="matchPair"></param>
        /// <returns></returns>
        public bool TryGetMatchPairs(int index, out (object userKeyValue, IEnumerable<EntityWithKey> matches) matchPair)
        {
            if (MatchPairs != null && MatchPairs.Length > index)
            {
                matchPair = MatchPairs[index];
                if (matchPair != default)
                {
                    return true;
                }
            }

            matchPair = default;
            return false;
        }

        #endregion

        #region Attachments

        /// <summary>
        /// Used by the save logic to store the freshly created file Id for entities with images or for attachments
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// Used by the save logic to store the freshly created file size for entities with images or for attachments
        /// </summary>
        public long FileSize { get; set; }

        #endregion

        /// <summary>
        /// Used for entity collections that undergo culling (empty records removed) to preserve the index in the original collection
        /// </summary>
        public int OriginalIndex { get; set; }

        /// <summary>
        /// Used by document export and import
        /// </summary>
        public Entity ManualLine { get; set; }
    }
}
