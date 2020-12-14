using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tellma.Entities;

namespace Tellma.Controllers.ImportExport
{
    /// <summary>
    /// Tree data structure where every level maps simple entity properties to column indices.
    /// Collection Navigation properties create the tree levels
    /// </summary>
    public class MappingInfo
    {
        // private fields
        private readonly Dictionary<string, PropertyMappingInfo> _simplePropsDic;
        private readonly Dictionary<string, MappingInfo> _collectionPropsDic; // Maps type name to mapping info

        /// <summary>
        /// This is the <see cref="CollectionPropertyMetadata"/> of the list property that this <see cref="MappingInfo"/> is based on.
        /// It is always null for the root mapping info
        /// </summary>
        public CollectionPropertyMetadata ParentCollectionPropertyMetadata { get; }


        /// <summary>
        /// Used by the parsing algorithm. This is the list where <see cref="Entity"/> lives
        /// </summary>
        public IList List { get; set; }

        /// <summary>
        /// Used by the parsing algorithm. This is the current entity being hydrated
        /// </summary>
        public Entity Entity { get; set; }

        // Other stuff

        public MappingInfo(TypeMetadata typeMetadata, IEnumerable<PropertyMappingInfo> simpleProps, IEnumerable<MappingInfo> collectionProps, CollectionPropertyMetadata parentCollectionPropMeta)
        {
            Metadata = typeMetadata ?? throw new ArgumentNullException(nameof(typeMetadata));
            SimpleProperties = simpleProps ?? throw new ArgumentNullException(nameof(simpleProps));
            _simplePropsDic = simpleProps.ToDictionary(p => p.Metadata.Descriptor.Name);

            CollectionProperties = collectionProps ?? throw new ArgumentNullException(nameof(collectionProps));
            _collectionPropsDic = collectionProps.ToDictionary(p => p.ParentCollectionPropertyMetadata.Descriptor.Name);

            ParentCollectionPropertyMetadata = parentCollectionPropMeta;
        }

        public TypeMetadata Metadata { get; set; }

        public IEnumerable<PropertyMappingInfo> SimpleProperties { get; set; }

        public IEnumerable<MappingInfo> CollectionProperties { get; set; }

        public IList GetOrCreateList(Entity entity)
        {
            if (ParentCollectionPropertyMetadata != null)
            {
                if (!(ParentCollectionPropertyMetadata.Descriptor.GetValue(entity) is IList list))
                {
                    list = ParentCollectionPropertyMetadata.CollectionTargetTypeMetadata.Descriptor.CreateList();
                    ParentCollectionPropertyMetadata.Descriptor.SetValue(entity, list);
                }

                return list;
            }
            else
            {
                throw new InvalidOperationException($"Bug: attempt to call {nameof(GetOrCreateList)} without the backing collection property metadata");
            }
        }

        public PropertyMappingInfo SimpleProperty(string name)
        {
            _simplePropsDic.TryGetValue(name, out PropertyMappingInfo result);
            return result;
        }

        public MappingInfo CollectionProperty(string name)
        {
            _collectionPropsDic.TryGetValue(name, out MappingInfo result);
            return result;
        }

        /// <summary>
        /// Retrieves the foreign key in this mapping and all its children
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ForeignKeyMappingInfo> GetForeignKeys()
        {
            var thisLevelFks = SimpleProperties.OfType<ForeignKeyMappingInfo>();
            var lowerLevelsFks = CollectionProperties.SelectMany(e => e.GetForeignKeys());
            return thisLevelFks.Concat(lowerLevelsFks);
        }

        /// <summary>
        /// Recursively clears <see cref="Entity"/> and <see cref="List"/> on the whole mapping tree
        /// </summary>
        public void ClearEntitiesAndLists()
        {
            Entity = null;
            List = null;

            foreach (var prop in CollectionProperties)
            {
                prop.ClearEntitiesAndLists();
            }
        }

        public int ColumnCount()
        {
            return MaxIndex() + 1;
        }

        private int MaxIndex()
        {
            int maxIndex = SimpleProperties.Max(prop => prop.Index);
            foreach (var prop in CollectionProperties)
            {
                maxIndex = Math.Max(maxIndex, prop.MaxIndex());
            }

            return maxIndex;
        }

        public ForeignKeyMappingInfo ParentIdProperty()
        {
            return SimpleProperty("ParentId") as ForeignKeyMappingInfo;
        }

        public IEnumerable<PropertyMappingInfo> AllPropertyMappings() => SimpleProperties
            .Concat(CollectionProperties.SelectMany(p => p.AllPropertyMappings()));

        public void NormalizeIndices()
        {
            var orderedProps = AllPropertyMappings().OrderBy(e => e.Index).Select((e, i) => (e, i));
            foreach (var (e, i) in orderedProps)
            {
                e.Index = i;
            }
        }

        public bool IsRoot { get; set; }
    }
}
