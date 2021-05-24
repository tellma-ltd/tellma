using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Tellma.Model.Common
{
    /// <summary>
    /// Describes a type of <see cref="Entity"/>. Offers a more performant alternative to traditional reflection.
    /// </summary>
    public class TypeDescriptor
    {
        private readonly Func<Entity> _create;
        private readonly Func<IList> _createList;
        private readonly IReadOnlyDictionary<string, PropertyDescriptor> _propertiesDic;
        private PropertyDescriptor _idProperty;

        /// <summary>
        /// The <see cref="Type"/> being described.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The type of the entity Id.
        /// </summary>
        public KeyType KeyType { get; }

        /// <summary>
        /// All mapped properties in the described <see cref="Entity"/> type.
        /// </summary>
        public IEnumerable<PropertyDescriptor> Properties { get; }

        /// <summary>
        /// All mapped properties in the described <see cref="Entity"/> type that are simple types (not navigation or collection).
        /// </summary>
        public IEnumerable<PropertyDescriptor> SimpleProperties { get; }

        /// <summary>
        /// All mapped navigation properties in the described <see cref="Entity"/> type that point to another <see cref="Entity"/>.
        /// </summary>
        public IEnumerable<NavigationPropertyDescriptor> NavigationProperties { get; }

        /// <summary>
        /// All mapped collection navigation properties in the described <see cref="Entity"/> type that point to <see cref="Entity"/> lists.
        /// </summary>
        public IEnumerable<CollectionPropertyDescriptor> CollectionProperties { get; }

        /// <summary>
        /// Creates a new instance of the described <see cref="Entity"/> type.
        /// </summary>
        /// <returns>The newly created <see cref="Entity"/>.</returns>
        public Entity Create() => _create();

        /// <summary>
        /// Creates an empty list of the <see cref="Entity"/> being described.
        /// </summary>
        /// <returns>The newly created <see cref="IList"/>.</returns>
        public IList CreateList() => _createList();

        /// <summary>
        /// Returns true if there is a property on this entity called Id.
        /// </summary>
        public bool HasId => KeyType == KeyType.None;

        /// <summary>
        /// Returns the property called "Id" or null if none is found.
        /// </summary>
        public PropertyDescriptor IdProperty => _idProperty ??= _propertiesDic["Id"];

        /// <summary>
        /// The name of this <see cref="Entity"/>. E.g. "Document".
        /// </summary>
        public string Name => Type.Name;

        /// <summary>
        /// Returns true if this <see cref="Entity"/> has a mapped property with the given name.
        /// </summary>
        public bool HasProperty(string propName) => _propertiesDic.ContainsKey(propName);

        /// <summary>
        /// Returns the <see cref="PropertyDescriptor"/> of the property with the given name.
        /// Returns null if no such property was found.
        /// </summary>
        public PropertyDescriptor Property(string propName)
        {
            _propertiesDic.TryGetValue(propName, out PropertyDescriptor result);
            return result;
        }

        /// <summary>
        /// Returns the <see cref="NavigationPropertyDescriptor"/> of the navigation property with the given name.
        /// Returns null if no such property was found.
        /// </summary>
        public NavigationPropertyDescriptor NavigationProperty(string propName)
        {
            _propertiesDic.TryGetValue(propName, out PropertyDescriptor result);
            return result as NavigationPropertyDescriptor;
        }

        /// <summary>
        /// Returns the <see cref="CollectionPropertyDescriptor"/> of the navigation property with the given name.
        /// Returns null if no such property was found.
        /// </summary>
        public CollectionPropertyDescriptor CollectionProperty(string propName)
        {
            _propertiesDic.TryGetValue(propName, out PropertyDescriptor result);
            return result as CollectionPropertyDescriptor;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public TypeDescriptor(Type type, Func<Entity> create, Func<IList> createList, IEnumerable<PropertyDescriptor> properties)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            _create = create ?? throw new ArgumentNullException(nameof(create));
            _createList = createList ?? throw new ArgumentNullException(nameof(create));
            Properties = properties ?? throw new ArgumentNullException(nameof(properties));
            SimpleProperties = properties.Where(e => e.GetType() == typeof(PropertyDescriptor));
            NavigationProperties = properties.OfType<NavigationPropertyDescriptor>();
            CollectionProperties = properties.OfType<CollectionPropertyDescriptor>();
            _propertiesDic = properties.ToDictionary(p => p.Name);

            // Set key type
            var prop = Property("Id");
            if (prop == null)
            {
                KeyType = KeyType.None;
            }
            else if ((Nullable.GetUnderlyingType(prop.Type) ?? prop.Type) == typeof(int))
            {
                KeyType = KeyType.Int;
            }
            else if (prop.Type == typeof(string))
            {
                KeyType = KeyType.String;
            }
            else
            {
                throw new InvalidOperationException("Only int and string Ids are permitted.");
            }
        }

        /// <summary>
        /// Forever cache of every descriptor ever requested through <see cref="Get(Type)"/>.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, TypeDescriptor> _cache = new();

        /// <summary>
        /// Creates and caches the type's <see cref="TypeDescriptor"/> using reflection.
        /// </summary>
        public static TypeDescriptor Get(Type type)
        {
            return _cache.GetOrAdd(type, (entityType) =>
            {
                #region Create

                Func<Entity> create;
                {
                    var ctorExp = entityType.GetConstructor(new Type[0]); // Document()
                    var newExp = Expression.New(ctorExp); // new Document()
                    var lambda = Expression.Lambda<Func<Entity>>(newExp); // () => new Document()
                    create = lambda.Compile();
                }

                #endregion

                #region Create List

                Func<IList> createList;
                {
                    var listType = typeof(List<>).MakeGenericType(entityType);
                    var ctorExp = listType.GetConstructor(new Type[0]); // List<Document>()
                    var newExp = Expression.New(ctorExp); // new List<Document>()
                    var lambda = Expression.Lambda<Func<IList>>(newExp); // () => new List<Document>()
                    createList = lambda.Compile();
                }

                #endregion

                #region Properties

                var propertiesDic = new Dictionary<string, PropertyDescriptor>();
                var properties = new List<PropertyDescriptor>();

                var propInfos = entityType.GetPropertiesBaseFirst(BindingFlags.Public | BindingFlags.Instance)
                    .Where(e => e.GetCustomAttribute<NotMappedAttribute>() == null);

                // The purpose of the OrderBy is to ensure that navigation properties come after their foreign key properties
                foreach (var propInfo in propInfos.OrderBy(p => (p.PropertyType.IsSubclassOf(typeof(Entity)) || p.PropertyType.IsList()) ? 1 : 0))
                {
                    var propType = propInfo.PropertyType;

                    #region Setter

                    // (e, v) => e.Name = (string)v
                    Action<Entity, object> setter;
                    {
                        var entityParam = Expression.Parameter(typeof(Entity), "e"); // e
                        var valueParam = Expression.Parameter(typeof(object), "v"); // v
                        var castEntity = Expression.Convert(entityParam, entityType); // (Account)e
                        var propertyAccess = Expression.MakeMemberAccess(castEntity, propInfo); // ((Account)e).Name
                        var convertedValue = Expression.Convert(valueParam, propType); // (string)v
                        var assignment = Expression.Assign(propertyAccess, convertedValue); // ((Account)e).Name = (string)v
                        var lambdaExp = Expression.Lambda<Action<Entity, object>>(assignment, entityParam, valueParam); // (e, v) => ((Account)e).Name = (string)v

                        setter = lambdaExp.Compile();
                    }

                    #endregion

                    #region Getter

                    // (e) => e.Name;
                    Func<Entity, object> getter;
                    {
                        var entityParam = Expression.Parameter(typeof(Entity), "e"); // e
                        var castEntity = Expression.Convert(entityParam, entityType); // (Account)e
                        var memberAccess = Expression.MakeMemberAccess(castEntity, propInfo); // ((Account)e).Name
                        var castMemberAccess = Expression.Convert(memberAccess, typeof(object)); // (object)((Account)e).Name
                        var lambdaExp = Expression.Lambda<Func<Entity, object>>(castMemberAccess, entityParam); // (e) => (object)((Account)e).Name

                        getter = lambdaExp.Compile();
                    }

                    #endregion

                    // Add property descriptor
                    PropertyDescriptor propDesc;
                    if (propInfo.PropertyType.IsList())
                    {
                        #region ForeignKeyDesc

                        var foreignKeyName = propInfo.GetCustomAttribute<ForeignKeyAttribute>()?.Name;
                        if (string.IsNullOrWhiteSpace(foreignKeyName))
                        {
                            // Developer mistake
                            throw new InvalidOperationException($"Collection property {propInfo.Name} on type {entityType.Name} is not adorned with the associated foreign key");
                        }

                        #endregion

                        #region getEntityDesc

                        Type collectionType = propInfo.PropertyType.GetGenericArguments().SingleOrDefault();
                        TypeDescriptor getCollectionEntityDescriptor() => Get(collectionType);

                        #endregion

                        // Collection
                        propDesc = new CollectionPropertyDescriptor(propInfo, setter, getter, foreignKeyName, getCollectionEntityDescriptor);
                    }
                    else if (propInfo.PropertyType.IsSubclassOf(typeof(Entity)))
                    {
                        #region IsParent

                        bool isParent = propInfo.Name == "Parent" && entityType.GetProperty("Node")?.PropertyType == typeof(HierarchyId);

                        #endregion

                        #region ForeignKeyDesc

                        var fkName = propInfo.GetCustomAttribute<ForeignKeyAttribute>()?.Name;
                        if (string.IsNullOrWhiteSpace(fkName))
                        {
                            // Developer mistake
                            throw new InvalidOperationException($"Navigation property {propInfo.Name} on type {entityType.Name} is not adorned with the associated foreign key.");
                        }

                        if (!propertiesDic.TryGetValue(fkName, out PropertyDescriptor foreignKeyDesc))
                        {
                            // Developer mistake
                            throw new InvalidOperationException($"Navigation property {propInfo.Name} on type {entityType.Name} is adorned with a foreign key '{fkName}' that doesn't exist.");
                        }

                        #endregion

                        #region getEntityDesc

                        TypeDescriptor getEntityDesc() => Get(propInfo.PropertyType);

                        #endregion

                        // Navigation
                        propDesc = new NavigationPropertyDescriptor(propInfo, setter, getter, isParent, foreignKeyDesc, getEntityDesc);
                    }
                    else
                    {
                        #region IndexPropertyName

                        // (e, v) => e.Name = (string)v
                        string indexPropName = null;
                        Action<Entity, int?> indexPropSetter = null;
                        Func<Entity, int?> indexPropGetter = null;
                        var selfRefAttribute = propInfo.GetCustomAttribute<SelfReferencingAttribute>(inherit: true);
                        if (selfRefAttribute != null)
                        {
                            indexPropName = selfRefAttribute.IndexPropertyName;
                            var indexPropInfo = entityType.GetProperty(indexPropName);
                            if (indexPropInfo == null || indexPropInfo.PropertyType != typeof(int?))
                            {
                                // Developer mistake
                                throw new InvalidOperationException($"Bug: Self referencing property {propInfo.Name} on type {entityType.Name} is adorned with an index property that doesn't exist or isn't of type nullable int.");
                            }

                            {
                                var entityParam = Expression.Parameter(typeof(Entity), "e"); // e
                                var valueParam = Expression.Parameter(typeof(int?), "v"); // v
                                var castEntity = Expression.Convert(entityParam, entityType); // (Center)e
                                var propertyAccess = Expression.MakeMemberAccess(castEntity, indexPropInfo); // ((Center)e).ParentIndex
                                var assignment = Expression.Assign(propertyAccess, valueParam); // ((Center)e).ParentIndex = v
                                var lambdaExp = Expression.Lambda<Action<Entity, int?>>(assignment, entityParam, valueParam); // (e, v) => ((Center)e).ParentIndex = v

                                indexPropSetter = lambdaExp.Compile();
                            }

                            {
                                var entityParam = Expression.Parameter(typeof(Entity), "e"); // e
                                var castEntity = Expression.Convert(entityParam, entityType); // (Center)e
                                var propertyAccess = Expression.MakeMemberAccess(castEntity, indexPropInfo); // ((Center)e).ParentIndex
                                var lambdaExp = Expression.Lambda<Func<Entity, int?>>(propertyAccess, entityParam); // (e) => ((Center)e).ParentIndex

                                indexPropGetter = lambdaExp.Compile();
                            }
                        }

                        #endregion

                        // Simple
                        propDesc = new PropertyDescriptor(propInfo, setter, getter, indexPropName, indexPropSetter, indexPropGetter);
                    }

                    propertiesDic.Add(propInfo.Name, propDesc);
                    properties.Add(propDesc);
                }

                #endregion

                // Prepare and return the entity descriptor
                var entityDesc = new TypeDescriptor(entityType, create, createList, properties);
                return entityDesc;
            });
        }

        /// <summary>
        /// Creates and caches the type's <see cref="TypeDescriptor"/> using reflection.
        /// </summary>
        public static TypeDescriptor Get<T>() => Get(typeof(T));
    }
}
