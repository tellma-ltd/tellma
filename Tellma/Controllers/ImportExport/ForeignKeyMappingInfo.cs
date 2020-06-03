using System;
using Tellma.Data.Queries;

namespace Tellma.Controllers.ImportExport
{
    public class ForeignKeyMappingInfo : PropertyMappingInfo
    {
        private PropertyMetadata _keyPropMetadata;

        /// <summary>
        /// The type of the property that is used as foreign key
        /// </summary>
        public KeyType KeyType { get; private set; }

        /// <summary>
        /// The <see cref="NavPropertyMetadata"/> for the navigation property associated with this foreign key
        /// </summary>
        public NavigationPropertyMetadata NavPropertyMetadata { get; set; }

        /// <summary>
        /// The property used as a key to query the navigation entities
        /// </summary>
        public PropertyMetadata KeyPropertyMetadata
        {
            get { return _keyPropMetadata; }
            set
            {
                _keyPropMetadata = value;

                // Set the KeyType too
                KeyType = KeyType.None;
                if (value != null)
                {
                    if (value.Descriptor.Type == typeof(string))
                    {
                        KeyType = KeyType.String;
                    }
                    else if (value.Descriptor.Type == typeof(int) || value.Descriptor.Type == typeof(int?))
                    {
                        KeyType = KeyType.Int;
                    }
                }
            }
        }

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
    }
}
