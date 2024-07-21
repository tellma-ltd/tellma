using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tellma.Api.Metadata;
using Tellma.Model.Common;
using Tellma.Utilities.Common;

namespace Tellma.Api.ImportExport
{
    /// <summary>
    /// Tree data structure where every level maps simple entity properties to column indices in an import file.
    /// Collection Navigation properties create the tree levels.
    /// </summary>
    public class MappingInfo
    {
        /// <summary>
        /// Maps a property name to a <see cref="PropertyMappingInfo"/> for fast retrieval in <see cref="SimplePropertyByName"/>.
        /// </summary>
        private Dictionary<string, IEnumerable<PropertyMappingInfo>> _simplePropsDic;

        /// <summary>
        /// Maps a property name to its <see cref="MappingInfo"/> for fast retrieval in <see cref="CollectionPropertyByName"/>.
        /// These are the collection navigation properties.
        /// </summary>
        private Dictionary<string, IEnumerable<MappingInfo>> _collectionPropsDic;

        /// <summary>
        /// The <see cref="CollectionPropertyMetadata"/> of the navigation collection that this <see cref="MappingInfo"/> is based on.
        /// It is always null for the root <see cref="MappingInfo"/>.
        /// </summary>
        public CollectionPropertyMetadata ParentCollectionPropertyMetadata { get; }

        /// <summary>
        /// The <see cref="CollectionPropertyMetadata"/> of the navigation collection for-save that this <see cref="MappingInfo"/> is based on.
        /// It is always null for the root <see cref="MappingInfo"/>.
        /// </summary>
        public CollectionPropertyMetadata ParentCollectionPropertyMetadataForSave { get; }

        /// <summary>
        /// Used by the parsing algorithm. This is the list where <see cref="Entity"/> lives.
        /// </summary>
        public IList List { get; set; }

        /// <summary>
        /// Used by the parsing algorithm. This is the entity currently getting hydrated.
        /// </summary>
        public Entity Entity { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="MappingInfo"/> class.
        /// </summary>
        /// <param name="typeMetadataForSave">The <see cref="TypeMetadata"/> of the collection for-save that this <see cref="MappingInfo"/> is based on.</param>
        /// <param name="typeMetadata">The <see cref="TypeMetadata"/> of the collection for-save that this <see cref="MappingInfo"/> is based on.</param>
        /// <param name="simpleProps">Mappings for the simple non-collection properties of the entities in this level.</param>
        /// <param name="collectionProps">Mappings for the collection properties of this entities ie the lower levels.</param>
        /// <param name="parentCollectionPropMetaForSave">This is the <see cref="CollectionPropertyMetadata"/> of the list 
        /// property that this <see cref="MappingInfo"/> is based on. It's always null for the root <see cref="MappingInfo"/>.</param>
        /// <param name="parentCollectionPropMeta"></param>
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
            CreateEntity = MetadataForSave.Descriptor.Create;

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
                Display = () => throw new InvalidOperationException(
                    $"Bug: attempt to call {nameof(Display)} without the backing collection property metadata");

                GetEntitiesForRead = (Entity _) => throw new InvalidOperationException(
                    $"Bug: attempt to call {nameof(GetOrCreateListForSave)} without the backing collection property metadata");
            }

            if (parentCollectionPropMetaForSave != null)
            {
                GetOrCreateListForSave = (Entity entity) =>
                {
                    if (parentCollectionPropMetaForSave.Descriptor.GetValue(entity) is not IList list)
                    {
                        list = parentCollectionPropMetaForSave.CollectionTargetTypeMetadata.Descriptor.CreateList();
                        parentCollectionPropMetaForSave.Descriptor.SetValue(entity, list);
                    }

                    return list;
                };
            }
            else
            {
                GetOrCreateListForSave = (Entity _) => throw new InvalidOperationException(
                    $"Bug: attempt to call {nameof(GetOrCreateListForSave)} without the backing collection property metadata");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingInfo"/> class cloning everything in 
        /// <paramref name="original"/> except <paramref name="simpleProps"/> and <paramref name="collectionProps"/>.
        /// </summary>
        public MappingInfo(
            MappingInfo original, 
            IEnumerable<PropertyMappingInfo> simpleProps, 
            IEnumerable<MappingInfo> collectionProps)
        {
            if (original is null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            MetadataForSave = original.MetadataForSave;
            SimpleProperties = simpleProps ?? throw new ArgumentNullException(nameof(simpleProps));
            CollectionProperties = collectionProps ?? throw new ArgumentNullException(nameof(collectionProps));
            ParentCollectionPropertyMetadataForSave = original.ParentCollectionPropertyMetadataForSave;
            ParentCollectionPropertyMetadata = original.ParentCollectionPropertyMetadata;
            CreateEntity = original.CreateEntity;
            GetEntitiesForRead = original.GetEntitiesForRead;
            GetOrCreateListForSave = original.GetOrCreateListForSave;
            Display = original.Display;
            Select = original.Select;
        }

        /// <summary>
        /// The <see cref="TypeMetadata"/> of the collection that this <see cref="MappingInfo"/> is based on.
        /// </summary>
        public TypeMetadata Metadata { get; }

        /// <summary>
        /// The <see cref="TypeMetadata"/> of the collection for-save that this <see cref="MappingInfo"/> is based on.
        /// </summary>
        public TypeMetadata MetadataForSave { get; }

        /// <summary>
        /// Mappings for the simple non-collection properties of the entities in this level.
        /// </summary>
        public IEnumerable<PropertyMappingInfo> SimpleProperties { get; set; } // Some APIs override this

        /// <summary>
        /// Mappings for the collection properties of this entities ie the lower levels.
        /// </summary>
        public IEnumerable<MappingInfo> CollectionProperties { get; set; } // Some APIs override this

        /// <summary>
        /// Creates an entity of the type of this <see cref="MappingInfo"/>.
        /// </summary>
        public Func<Entity> CreateEntity { get; set; }

        /// <summary>
        /// Created an entity with the given <see cref="row"/> and this <see cref="MappingInfo"/> set in its metadata.
        /// </summary>
        /// <param name="row">The number of the imported sheet row that triggered the creation of this entity.</param>
        /// <returns>The created entity.</returns>
        public Entity CreateBaseEntity(int row)
        {
            var entity = CreateEntity();

            entity.EntityMetadata.RowNumber = row;
            entity.EntityMetadata.MappingInfo = this;

            return entity;
        }

        /// <summary>
        /// Given a parent entity, this function returns the contents of the collection property represeted
        /// by this <see cref="MappingInfo"/>.
        /// </summary>
        public Func<Entity, IEnumerable> GetEntitiesForRead { get; set; }

        /// <summary>
        /// Given a parent entity, this function creates a list of the entities of the current level's type. 
        /// This is used to initialize the collection navigation property represented by this <see cref="MappingInfo"/>.
        /// </summary>
        public Func<Entity, IList> GetOrCreateListForSave { get; set; }

        /// <summary>
        /// The display label of this collection property.
        /// </summary>
        public Func<string> Display { get; set; }

        /// <summary>
        /// Used internally by the import algorithm to track self referencing entities.
        /// </summary>
        internal bool IsRoot { get; set; }

        /// <summary>
        /// When construting the select, this represents the select step of the current collection (null for root).
        /// </summary>
        internal string Select { get; set; }

        // Methods

        /// <summary>
        /// Helper method to keep the C# compiler happy.
        /// </summary>
        private static IEnumerable<T> Enumerable<T>(IEnumerable<T> e) => e;

        /// <summary>
        /// Returns all the simple properties with the given name (e.g. "PostingDate").
        /// </summary>
        public IEnumerable<PropertyMappingInfo> SimplePropertiesByName(string name)
        {
            _simplePropsDic ??= SimpleProperties
                .GroupBy(e => e.Metadata.Descriptor.Name)
                .ToDictionary(g => g.Key, g => Enumerable(g));

            _simplePropsDic.TryGetValue(name, out IEnumerable<PropertyMappingInfo> result);
            return result;
        }

        /// <summary>
        /// Retrieves the single Simple Property that has the given name (or null if non was found).
        /// Throws and exception if multiple matches were found.
        /// </summary>
        public PropertyMappingInfo SimplePropertyByName(string name)
        {
            return SimplePropertiesByName(name)?.SingleOrDefault();
        }

        /// <summary>
        /// Returns all the collection properties with the given name (e.g. "Lines").
        /// </summary>
        public IEnumerable<MappingInfo> CollectionPropertiesByName(string name)
        {
            _collectionPropsDic ??= CollectionProperties
                .GroupBy(e => e.ParentCollectionPropertyMetadata?.Descriptor?.Name ?? throw new InvalidOperationException($"Bug: {nameof(ParentCollectionPropertyMetadata)} was null in {nameof(MappingInfo)} for {e.MetadataForSave.SingularDisplay()}"))
                .ToDictionary(g => g.Key, g => Enumerable(g));

            _collectionPropsDic.TryGetValue(name, out IEnumerable<MappingInfo> result);
            return result;
        }

        /// <summary>
        /// Retrieves the single Collection Property that has the given name (or null if non was found).
        /// Throws and exception if multiple matches were found.
        /// </summary>
        public MappingInfo CollectionPropertyByName(string name)
        {
            return CollectionPropertiesByName(name)?.SingleOrDefault();
        }

        /// <summary>
        /// Determines if a simple property by the given <paramref name="name"/> exists.
        /// </summary>
        public bool HasSimplePropertyByName(string name) => SimplePropertyByName(name) != null;

        /// <summary>
        /// Retrieves the foreign keys in this mapping and all its children.
        /// </summary>
        public IEnumerable<ForeignKeyMappingInfo> GetForeignKeys()
        {
            var thisLevelFks = SimpleProperties.OfType<ForeignKeyMappingInfo>();
            var lowerLevelsFks = CollectionProperties.SelectMany(e => e.GetForeignKeys());

            return thisLevelFks.Concat(lowerLevelsFks);
        }

        /// <summary>
        /// Recursively clears <see cref="Entity"/> and <see cref="List"/> on the whole mapping tree.
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

        /// <summary>
        /// Removes any gaps in the indices of the <see cref="PropertyMappingInfo"/> in this level and all the levels below it.
        /// </summary>
        public void NormalizeIndices()
        {
            var orderedProps = AllPropertyMappings().OrderBy(e => e.Index).Indexed();
            foreach (var (e, i) in orderedProps)
            {
                e.Index = i;
            }
        }

        /// <summary>
        /// The number of columns needed to fit all the <see cref="PropertyMappingInfo"/>s in this level and all the levels below it.
        /// </summary>
        public int ColumnCount()
        {
            return MaxIndex() + 1;
        }

        /// <summary>
        /// The maximum <see cref="PropertyMappingInfo.Index"/> in the entire tree.
        /// </summary>
        private int MaxIndex()
        {
            int maxIndex = SimpleProperties.Max(prop => prop.Index);
            foreach (var prop in CollectionProperties)
            {
                maxIndex = Math.Max(maxIndex, prop.MaxIndex());
            }

            return maxIndex;
        }

        /// <summary>
        /// All the <see cref="PropertyMappingInfo"/>s in this level and all the levels below it concatenated in a single <see cref="IEnumerable{T}"/>.
        /// </summary>
        private IEnumerable<PropertyMappingInfo> AllPropertyMappings() => SimpleProperties
            .Concat(CollectionProperties.SelectMany(p => p.AllPropertyMappings()));

    }
}
