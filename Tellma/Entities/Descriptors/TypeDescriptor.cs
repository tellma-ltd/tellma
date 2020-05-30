using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Tellma.Data.Queries;
using Tellma.Services.Utilities;

namespace Tellma.Entities.Descriptors
{
    /// <summary>
    /// Describes a type of <see cref="Entity"/>. Offers a more performant alternative to traditional reflection
    /// </summary>
    public class TypeDescriptor
    {
        private readonly Func<Entity> _create;
        private readonly Func<IList> _createList;
        private readonly IReadOnlyDictionary<string, PropertyDescriptor> _propertiesDic;

        /// <summary>
        /// The <see cref="Type"/> being described
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The type of the entity Id
        /// </summary>
        public KeyType KeyType { get; }

        /// <summary>
        /// All mapped properties on this entity.
        /// </summary>
        public IEnumerable<PropertyDescriptor> Properties { get; }

        /// <summary>
        /// All mapped properties on this entity that are simple types (not navigation or collection)
        /// </summary>
        public IEnumerable<PropertyDescriptor> SimpleProperties { get; }

        /// <summary>
        /// All mapped navigation properties on this entity that point to another <see cref="Entity"/>
        /// </summary>
        public IEnumerable<NavigationPropertyDescriptor> NavigationProperties { get; }

        /// <summary>
        /// All mapped collection navigation properties on this entity that point to <see cref="Entity"/> lists
        /// </summary>
        public IEnumerable<CollectionPropertyDescriptor> CollectionProperties { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="Entity"/> being described
        /// </summary>
        /// <returns>The newly created <see cref="Entity"/></returns>
        public Entity Create() => _create();

        /// <summary>
        /// Creates an empty list of the <see cref="Entity"/> being described
        /// </summary>
        /// <returns>The newly created <see cref="IList"/></returns>
        public IList CreateList() => _createList();

        /// <summary>
        /// Returns true if there is a property on this entity called Id
        /// </summary>
        public bool HasId => _propertiesDic.ContainsKey("Id");

        /// <summary>
        /// The name of this <see cref="Entity"/>. E.g. "Document"
        /// </summary>
        public string Name => Type.Name;

        /// <summary>
        /// Returns true if this <see cref="Entity"/> has a mapped property with the given name
        /// </summary>
        public bool HasProperty(string propName)
        {
            return _propertiesDic.ContainsKey(propName);
        }

        /// <summary>
        /// Returns the <see cref="PropertyDescriptor"/> of the property with the given name.
        /// Returns null if no such property was found
        /// </summary>
        public PropertyDescriptor Property(string propName)
        {
            _propertiesDic.TryGetValue(propName, out PropertyDescriptor result);
            return result;
        }

        /// <summary>
        /// Returns the <see cref="NavigationPropertyDescriptor"/> of the navigation property with the given name.
        /// Returns null if no such property was found
        /// </summary>
        public NavigationPropertyDescriptor NavigationProperty(string propName)
        {
            _propertiesDic.TryGetValue(propName, out PropertyDescriptor result);
            return result as NavigationPropertyDescriptor;
        }

        /// <summary>
        /// Returns the <see cref="CollectionPropertyDescriptor"/> of the navigation property with the given name.
        /// Returns null if no such property was found
        /// </summary>
        public CollectionPropertyDescriptor CollectionProperty(string propName)
        {
            _propertiesDic.TryGetValue(propName, out PropertyDescriptor result);
            return result as CollectionPropertyDescriptor;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public TypeDescriptor(
            Type type,
            Func<Entity> create,
            Func<IList> createList,
            IEnumerable<PropertyDescriptor> properties)
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
                throw new InvalidOperationException("Only int and string Ids are permitted");
            }
        }

        /// <summary>
        /// Forever cache of every descriptor ever requested through <see cref="Get(Type)"/>
        /// </summary>
        private static readonly ConcurrentDictionary<Type, TypeDescriptor> _cache = new ConcurrentDictionary<Type, TypeDescriptor>();

        /// <summary>
        /// Uses reflection to create and cache <see cref="TypeDescriptor"/>s.
        /// Those are a much faster alternative to reflection and can be used e.g. to create entities and set and get their properties.
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
                    #region Type && Name

                    var propType = propInfo.PropertyType;
                    var name = propInfo.Name;

                    #endregion

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
                        Func<TypeDescriptor> getCollectionEntityDescriptor = () => Get(collectionType);

                        #endregion

                        // Collection
                        propDesc = new CollectionPropertyDescriptor(propInfo, name, setter, getter, foreignKeyName, getCollectionEntityDescriptor);
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
                            throw new InvalidOperationException($"Navigation property {propInfo.Name} on type {entityType.Name} is not adorned with the associated foreign key");
                        }

                        if (!propertiesDic.TryGetValue(fkName, out PropertyDescriptor foreignKeyDesc))
                        {
                            // Developer mistake
                            throw new InvalidOperationException($"Navigation property {propInfo.Name} on type {entityType.Name} is adorned with a foreign key '{fkName}' that doesn't exist");
                        }

                        #endregion

                        #region getEntityDesc

                        Func<TypeDescriptor> getEntityDesc = () => Get(propInfo.PropertyType);

                        #endregion

                        // Navigation
                        propDesc = new NavigationPropertyDescriptor(propInfo, name, setter, getter, isParent, foreignKeyDesc, getEntityDesc);
                    }
                    else
                    {
                        #region MaxLength

                        int maxLength = -1;
                        var stringLengthAttribute = propInfo.GetCustomAttribute<StringLengthAttribute>(inherit: true);
                        if (stringLengthAttribute != null)
                        {
                            maxLength = stringLengthAttribute.MaximumLength;
                        }

                        #endregion

                        // Simple
                        propDesc = new PropertyDescriptor(propInfo, name, setter, getter, maxLength);
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
        /// Uses reflection to create and cache <see cref="TypeDescriptor"/>s.
        /// Those are a much faster alternative to reflection and can be used e.g. to create entities and set and get their properties.
        /// </summary>
        public static TypeDescriptor Get<T>() => Get(typeof(T));
    }
}
