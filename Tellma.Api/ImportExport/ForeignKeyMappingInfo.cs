using System;
using Tellma.Api.Metadata;
using Tellma.Model.Common;

namespace Tellma.Api.ImportExport
{
    /// <summary>
    /// Maps a foreign key property to a column index in the imported/exported sheet.
    /// </summary>
    public class ForeignKeyMappingInfo : PropertyMappingInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ForeignKeyMappingInfo"/> class.
        /// </summary>
        /// <param name="metadata">The <see cref="PropertyMetadata"/> of the mapped property on the read entity.</param>
        /// <param name="metadataForSave">The <see cref="PropertyMetadata"/> of the mapped property on the for-save entity.</param>
        /// <param name="navPropertyMetadata">The <see cref="PropertyMetadata"/> of the navigation property associated with this foreign key.</param>
        /// <param name="keyPropertyMetadata">The <see cref="PropertyMetadata"/> of the key property used to query the navigation entities.</param>
        public ForeignKeyMappingInfo(
            PropertyMetadata metadata, 
            PropertyMetadata metadataForSave, 
            NavigationPropertyMetadata navPropertyMetadata,
            PropertyMetadata keyPropertyMetadata) : base(metadata, metadataForSave)
        {
            NavPropertyMetadata = navPropertyMetadata ?? throw new ArgumentNullException(nameof(navPropertyMetadata));
            KeyPropertyMetadata = keyPropertyMetadata ?? throw new ArgumentNullException(nameof(keyPropertyMetadata));
            KeyType = GetKeyType(keyPropertyMetadata);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForeignKeyMappingInfo"/> class. 
        /// Clones everything in <paramref name="original"/> except for <see cref="ForeignKeyMappingInfo.Index"/>.
        /// </summary>
        public ForeignKeyMappingInfo(ForeignKeyMappingInfo original, PropertyMetadata keyPropertyMetadata) : base(original)
        {
            if (original is null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            NavPropertyMetadata = original.NavPropertyMetadata;
            KeyPropertyMetadata = keyPropertyMetadata ?? throw new ArgumentNullException(nameof(keyPropertyMetadata));
            KeyType = GetKeyType(keyPropertyMetadata);
        }

        /// <summary>
        /// The type of the property that is used as foreign key.
        /// </summary>
        public KeyType KeyType { get; }

        /// <summary>
        /// The <see cref="NavPropertyMetadata"/> for the navigation property associated with this foreign key.
        /// </summary>
        public NavigationPropertyMetadata NavPropertyMetadata { get; }

        /// <summary>
        /// The property used as a key to query the navigation entities.
        /// </summary>
        public PropertyMetadata KeyPropertyMetadata { get; }

        /// <summary>
        /// Syntactic sugar for: NavPropertyMetadata.EntityMetadata.Descriptor.Type.
        /// </summary>
        public Type TargetType => NavPropertyMetadata.TargetTypeMetadata.Descriptor.Type;

        /// <summary>
        /// Syntactic sugar for: NavPropertyMetadata.EntityMetadata.DefinitionId.
        /// </summary>
        public int? TargetDefId => NavPropertyMetadata.TargetTypeMetadata.DefinitionId;

        /// <summary>
        /// Syntactic sugar for: KeyPropertyMetadata.Descriptor.Name != "Id".
        /// </summary>
        public bool NotUsingIdAsKey => KeyPropertyMetadata.Descriptor.Name != "Id";

        /// <summary>
        /// Syntactic sugar for: Metadata.Descriptor.IsSelfReferencing.
        /// </summary>
        public bool IsSelfReferencing => Metadata.Descriptor.IsSelfReferencing;

        /// <summary>
        /// For self referencing properties, stores the index of matches in <see cref="Entity.EntityMetadata.MatchPairs"/>.
        /// </summary>
        public int EntityMetadataMatchesIndex { get; set; }

        /// <summary>
        /// Helper function
        /// </summary>
        private static KeyType GetKeyType(PropertyMetadata keyProp)
        {
            // Set the key type
            if (keyProp.Descriptor.Type == typeof(string))
            {
                return KeyType.String;
            }
            else
            {
                return KeyType.Int;
            }
        }
    }
}
