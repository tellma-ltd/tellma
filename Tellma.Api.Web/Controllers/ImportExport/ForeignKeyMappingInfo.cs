using System;
using Tellma.Data.Queries;

namespace Tellma.Controllers.ImportExport
{
    public class ForeignKeyMappingInfo : PropertyMappingInfo
    {
        public ForeignKeyMappingInfo(PropertyMetadata metadata, PropertyMetadata metadataForSave, NavigationPropertyMetadata navPropertyMetadata, PropertyMetadata keyPropertyMetadata) : base(metadata, metadataForSave)
        {
            NavPropertyMetadata = navPropertyMetadata ?? throw new ArgumentNullException(nameof(navPropertyMetadata));
            KeyPropertyMetadata = keyPropertyMetadata ?? throw new ArgumentNullException(nameof(keyPropertyMetadata));
            KeyType = GetKeyType(keyPropertyMetadata);
        }

        /// <summary>
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
        /// Helper function
        /// </summary>
        /// <param name="keyProp"></param>
        /// <returns></returns>
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

        /// <summary>
        /// The type of the property that is used as foreign key
        /// </summary>
        public KeyType KeyType { get; }

        /// <summary>
        /// The <see cref="NavPropertyMetadata"/> for the navigation property associated with this foreign key
        /// </summary>
        public NavigationPropertyMetadata NavPropertyMetadata { get; }

        /// <summary>
        /// The property used as a key to query the navigation entities
        /// </summary>
        public PropertyMetadata KeyPropertyMetadata { get; }

        /// <summary>
        /// Syntactic sugar for: NavPropertyMetadata.EntityMetadata.Descriptor.Type
        /// </summary>
        public Type TargetType => NavPropertyMetadata.TargetTypeMetadata.Descriptor.Type;

        /// <summary>
        /// Syntactic sugar for: NavPropertyMetadata.EntityMetadata.DefinitionId
        /// </summary>
        public int? TargetDefId => NavPropertyMetadata.TargetTypeMetadata.DefinitionId;

        /// <summary>
        /// Syntactic sugar for: KeyPropertyMetadata.Descriptor.Name != "Id"
        /// </summary>
        public bool NotUsingIdAsKey => KeyPropertyMetadata.Descriptor.Name != "Id";

        /// <summary>
        /// Syntactic sugar for: Metadata.Descriptor.IsSelfReferencing
        /// </summary>
        public bool IsSelfReferencing => Metadata.Descriptor.IsSelfReferencing;

        /// <summary>
        /// For self referencing properties, stores the index of matches in <see cref="Entities.EntityMetadata.MatchPairs"/>
        /// </summary>
        public int EntityMetadataMatchesIndex { get; set; }
    }
}
