using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private Dictionary<string, PropertyMappingInfo> _simplePropsDic;
        private Dictionary<string, MappingInfo> _collectionPropsDic; // Maps type name to mapping info

        /// <summary>
        /// This is the <see cref="CollectionPropertyMetadata"/> of the list property that this <see cref="MappingInfo"/> is based on.
        /// It is always null for the root mapping info
        /// </summary>
        public CollectionPropertyMetadata ParentCollectionPropertyMetadata { get; }

        /// <summary>
        /// This is the <see cref="CollectionPropertyMetadata"/> of the list property for-save that this <see cref="MappingInfo"/> is based on.
        /// It is always null for the root mapping info
        /// </summary>
        public CollectionPropertyMetadata ParentCollectionPropertyMetadataForSave { get; }

        /// <summary>
        /// Used by the parsing algorithm. This is the list where <see cref="Entity"/> lives
        /// </summary>
        public IList List { get; set; }

        /// <summary>
        /// Used by the parsing algorithm. This is the current entity being hydrated
        /// </summary>
        public Entity Entity { get; set; }

        // Other stuff

        public MappingInfo(
            TypeMetadata typeMetadataForSave,
            TypeMetadata typeMetadata,
            IEnumerable<PropertyMappingInfo> simpleProps,
            IEnumerable<MappingInfo> collectionProps,
            CollectionPropertyMetadata parentCollectionPropMetaForSave,
            CollectionPropertyMetadata parentCollectionPropMeta)
        {
            MetadataForSave = typeMetadataForSave ?? throw new ArgumentNullException(nameof(typeMetadataForSave));
            Metadata = typeMetadata ?? throw new ArgumentNullException(nameof(typeMetadata));

            SimpleProperties = simpleProps ?? throw new ArgumentNullException(nameof(simpleProps));
            CollectionProperties = collectionProps ?? throw new ArgumentNullException(nameof(collectionProps));

            ParentCollectionPropertyMetadataForSave = parentCollectionPropMetaForSave;
            ParentCollectionPropertyMetadata = parentCollectionPropMeta;

            CreateEntity = (int rowNumber) =>
            {
                var entity = MetadataForSave.Descriptor.Create();
                entity.EntityMetadata.RowNumber = rowNumber; // For validation reporting
                return entity;
            };

            // All these can be overridden
            if (parentCollectionPropMeta != null)
            {
                Display = parentCollectionPropMeta.Display;

                GetEntitiesForRead = (Entity entity) =>
                {
                    return parentCollectionPropMeta.Descriptor.GetValue(entity) as IList ??
                        parentCollectionPropMeta.CollectionTargetTypeMetadata.Descriptor.CreateList();
                };

                Select = parentCollectionPropMeta.Descriptor.Name;
            }
            else
            {
                // It's up to the user to 
                Display = () => throw new InvalidOperationException($"Bug: attempt to call {nameof(Display)} without the backing collection property metadata");
                GetEntitiesForRead = (Entity _) => throw new InvalidOperationException($"Bug: attempt to call {nameof(GetOrCreateListForSave)} without the backing collection property metadata");
            }

            if (parentCollectionPropMetaForSave != null)
            {
                GetOrCreateListForSave = (Entity entity) =>
                {
                    if (!(parentCollectionPropMetaForSave.Descriptor.GetValue(entity) is IList list))
                    {
                        list = parentCollectionPropMetaForSave.CollectionTargetTypeMetadata.Descriptor.CreateList();
                        parentCollectionPropMetaForSave.Descriptor.SetValue(entity, list);
                    }

                    return list;
                };
            }
            else
            {
                GetOrCreateListForSave = (Entity _) => throw new InvalidOperationException($"Bug: attempt to call {nameof(GetOrCreateListForSave)} without the backing collection property metadata");
            }
        }

        /// <summary>
        /// Clones everything in <paramref name="original"/>.
        /// </summary>
        public MappingInfo(MappingInfo original, IEnumerable<PropertyMappingInfo> simpleProps, IEnumerable<MappingInfo> collectionProps)
        {
            if (original is null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            MetadataForSave = original.MetadataForSave;
            SimpleProperties = simpleProps ?? throw new ArgumentNullException(nameof(simpleProps));
            CollectionProperties = collectionProps ?? throw new ArgumentNullException(nameof(collectionProps));
            CreateEntity = original.CreateEntity;
            GetEntitiesForRead = original.GetEntitiesForRead;
            GetOrCreateListForSave = original.GetOrCreateListForSave;
            Display = original.Display;
            Select = original.Select;
        }

        // Properties

        public TypeMetadata Metadata { get; }

        public TypeMetadata MetadataForSave { get; }

        public IEnumerable<PropertyMappingInfo> SimpleProperties { get; set; } // Some APIs override this

        public IEnumerable<MappingInfo> CollectionProperties { get; set; } // Some APIs override this

        public Func<int, Entity> CreateEntity { get; set; }

        public Func<Entity, IEnumerable> GetEntitiesForRead { get; set; }

        public Func<Entity, IList> GetOrCreateListForSave { get; set; }

        public bool IsRoot { get; set; }

        public Func<string> Display { get; set; }

        /// <summary>
        /// When construting the select, this represents the select step of the current collection (null for root)
        /// </summary>
        public string Select { get; set; }

        // Methods

        public PropertyMappingInfo SimpleProperty(string name)
        {
            _simplePropsDic ??= SimpleProperties.ToDictionary(p => p.Metadata.Descriptor.Name);
            _simplePropsDic.TryGetValue(name, out PropertyMappingInfo result);
            return result;
        }

        public MappingInfo CollectionProperty(string name)
        {
            _collectionPropsDic ??= CollectionProperties.ToDictionary(p => p.ParentCollectionPropertyMetadata?.Descriptor?.Name ??
                    throw new InvalidOperationException($"Bug: {nameof(ParentCollectionPropertyMetadata)} was null in {nameof(MappingInfo)} for {p.MetadataForSave.SingularDisplay()}"));

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
    }
}
