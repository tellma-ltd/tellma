//using Microsoft.Extensions.Localization;
//using System;
//using System.Collections;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using Tellma.Controllers.Dto;
//using Tellma.Data.Queries;
//using Tellma.Entities;
//using Tellma.Services.MultiTenancy;
//using Tellma.Services.Utilities;

//namespace Tellma.Controllers.ModelMetadata
//{
//    public class MetadataProvider
//    {
//        private readonly ITenantIdAccessor _tenantIdAccessor;
//        private readonly IDefinitionsCache _definitionsCache;
//        private readonly ISettingsCache _settingsCache;
//        private readonly IStringLocalizer<Strings> _localizer;
//        private readonly IServiceProvider _serviceProvider;

//        private static readonly ConcurrentDictionary<CacheKey, EntityDescriptor> _cache = new ConcurrentDictionary<CacheKey, EntityDescriptor>();

//        public MetadataProvider(
//            ITenantIdAccessor tenantIdAccessor,
//            IDefinitionsCache definitionsCache,
//            ISettingsCache settingsCache,
//            IStringLocalizer<Strings> localizer,
//            IServiceProvider serviceProvider)
//        {
//            _tenantIdAccessor = tenantIdAccessor;
//            _definitionsCache = definitionsCache;
//            _settingsCache = settingsCache;
//            _localizer = localizer;
//            _serviceProvider = serviceProvider;
//        }

//        public EntityDescriptor GetDescriptor(Type entityType, string definitionId = null, int? tenantId = null)
//        {

//            // Database Id
//            int databaseId = tenantId ?? _tenantIdAccessor.GetTenantId();

//            // Definition Id
//            if (string.IsNullOrWhiteSpace(definitionId))
//            {
//                definitionId = null;
//            }

//            DefinitionsForClient defs = null;
//            if (definitionId != null)
//            {
//                defs = _definitionsCache.GetDefinitionsIfCached(databaseId)?.Data ??
//                    throw new InvalidOperationException($"Bug: The definitions cache is empty for tenantId = {databaseId}");
//            }

//            SettingsForClient settings = _settingsCache.GetSettingsIfCached(databaseId)?.Data ??
//                    throw new InvalidOperationException($"Bug: The settings cache is empty for tenantId = {databaseId}");

//            // Cache key
//            var key = new CacheKey
//            {
//                TenantId = databaseId,
//                EntityType = entityType,
//                DefinitionId = definitionId,
//                Settings = settings,
//                Definitions = defs,
//            };

//            return _cache.GetOrAdd(key, (pair) =>
//            {
//                int tenantId = pair.TenantId;
//                Type entityType = pair.EntityType;

//                //-////////////////////////////////////////
//                // Create
//                //-////////////////////////////////////////
//                Func<Entity> create;
//                {
//                    var ctorExp = entityType.GetConstructor(new Type[0]); // Document()
//                    var newExp = Expression.New(ctorExp); // new Document()
//                    var lambda = Expression.Lambda<Func<Entity>>(newExp); // () => new Document()
//                    create = lambda.Compile();
//                }

//                //-////////////////////////////////////////
//                // Create List
//                //-////////////////////////////////////////
//                Func<IList> createList;
//                {
//                    var listType = typeof(List<>).MakeGenericType(entityType);
//                    var ctorExp = listType.GetConstructor(new Type[0]); // List<Document>()
//                    var newExp = Expression.New(ctorExp); // new List<Document>()
//                    var lambda = Expression.Lambda<Func<IList>>(newExp); // () => new List<Document>()
//                    createList = lambda.Compile();
//                }

//                //-////////////////////////////////////////
//                // Properties
//                //-////////////////////////////////////////
//                var properties = new Dictionary<string, PropertyDescriptor>();
//                var propertyList = new List<PropertyDescriptor>();

//                var propInfos = entityType.GetPropertiesBaseFirst(BindingFlags.Public | BindingFlags.Instance)
//                    .Where(e => e.GetCustomAttribute<NotMappedAttribute>() == null);

//                foreach (var propInfo in propInfos)
//                {
//                    //-////////////////////////////////////////
//                    // Display
//                    //-////////////////////////////////////////

//                    // Default function
//                    Func<string> display = () => propInfo.Name;

//                    // Use DisplayAttribute if present
//                    var displayAtt = propInfo.GetCustomAttribute<DisplayAttribute>();
//                    if (displayAtt != null)
//                    {
//                        display = () => _localizer[displayAtt.Name];
//                    }

//                    // Check MultilingualDisplay
//                    var multilingualAtt = propInfo.GetCustomAttribute<MultilingualDisplayAttribute>();
//                    if (multilingualAtt != null && (settings.SecondaryLanguageId != null || settings.TernaryLanguageId != null))
//                    {
//                        var unilingualDisplay = display;
//                        display = multilingualAtt.Language switch
//                        {
//                            Language.Primary => () => $"{unilingualDisplay()} ({settings.PrimaryLanguageSymbol})",
//                            Language.Secondary => () => $"{unilingualDisplay()} ({settings.SecondaryLanguageSymbol})",
//                            Language.Ternary => () => $"{unilingualDisplay()} ({settings.TernaryLanguageSymbol})",
//                            _ => throw new InvalidOperationException($"Unknown Language {multilingualAtt.Language}") // Future proofing
//                        };
//                    }

//                    // Override default from definition
//                    if (defs != null)
//                    {
//                        var defaultDisplay = display;

//                        switch (entityType.Name)
//                        {
//                            case nameof(Resource):
//                            case nameof(ResourceForSave):
//                                {
//                                    if (!defs.Resources.TryGetValue(definitionId, out ResourceDefinitionForClient def))
//                                    {
//                                        var msg = _localizer[$"Error_ResourceDefinition0CouldNotBeFound", definitionId];
//                                        throw new BadRequestException(msg);
//                                    }

//                                    display = propInfo.Name switch
//                                    {
//                                        nameof(Resource.Identifier) => ResourceDisplay(settings, def.IdentifierVisibility, def.IdentifierLabel, def.IdentifierLabel2, def.IdentifierLabel3, defaultDisplay),
//                                        nameof(Resource.Currency) => ResourceDisplay(settings, def.CurrencyVisibility, def.CurrencyLabel, def.CurrencyLabel2, def.CurrencyLabel3, defaultDisplay),
//                                        nameof(Resource.CurrencyId) => ResourceDisplay(settings, def.CurrencyVisibility, def.CurrencyLabel, def.CurrencyLabel2, def.CurrencyLabel3, defaultDisplay),
//                                        nameof(Resource.MonetaryValue) => ResourceDisplay(settings, def.MonetaryValueVisibility, def.MonetaryValueLabel, def.MonetaryValueLabel2, def.MonetaryValueLabel3, defaultDisplay),
//                                        nameof(Resource.AvailableSince) => ResourceDisplay(settings, def.AvailableSinceVisibility, def.AvailableSinceLabel, def.AvailableSinceLabel2, def.AvailableSinceLabel3, defaultDisplay),
//                                        nameof(Resource.AvailableTill) => ResourceDisplay(settings, def.AvailableTillVisibility, def.AvailableTillLabel, def.AvailableTillLabel2, def.AvailableTillLabel3, defaultDisplay),
//                                        nameof(Resource.Decimal1) => ResourceDisplay(settings, def.Decimal1Visibility, def.Decimal1Label, def.Decimal1Label2, def.Decimal1Label3, defaultDisplay),
//                                        nameof(Resource.Decimal2) => ResourceDisplay(settings, def.Decimal2Visibility, def.Decimal2Label, def.Decimal2Label2, def.Decimal2Label3, defaultDisplay),
//                                        nameof(Resource.Int1) => ResourceDisplay(settings, def.Int1Visibility, def.Int1Label, def.Int1Label2, def.Int1Label3, defaultDisplay),
//                                        nameof(Resource.Int2) => ResourceDisplay(settings, def.Int2Visibility, def.Int2Label, def.Int2Label2, def.Int2Label3, defaultDisplay),
//                                        nameof(Resource.Lookup1) => ResourceDisplay(settings, def.Lookup1Visibility, def.Lookup1Label, def.Lookup1Label2, def.Lookup1Label3, defaultDisplay),
//                                        nameof(Resource.Lookup1Id) => ResourceDisplay(settings, def.Lookup1Visibility, def.Lookup1Label, def.Lookup1Label2, def.Lookup1Label3, defaultDisplay),
//                                        nameof(Resource.Lookup2) => ResourceDisplay(settings, def.Lookup2Visibility, def.Lookup2Label, def.Lookup2Label2, def.Lookup2Label3, defaultDisplay),
//                                        nameof(Resource.Lookup2Id) => ResourceDisplay(settings, def.Lookup2Visibility, def.Lookup2Label, def.Lookup2Label2, def.Lookup2Label3, defaultDisplay),
//                                        nameof(Resource.Lookup3) => ResourceDisplay(settings, def.Lookup3Visibility, def.Lookup3Label, def.Lookup3Label2, def.Lookup3Label3, defaultDisplay),
//                                        nameof(Resource.Lookup3Id) => ResourceDisplay(settings, def.Lookup3Visibility, def.Lookup3Label, def.Lookup3Label2, def.Lookup3Label3, defaultDisplay),
//                                        nameof(Resource.Lookup4) => ResourceDisplay(settings, def.Lookup4Visibility, def.Lookup4Label, def.Lookup4Label2, def.Lookup4Label3, defaultDisplay),
//                                        nameof(Resource.Lookup4Id) => ResourceDisplay(settings, def.Lookup4Visibility, def.Lookup4Label, def.Lookup4Label2, def.Lookup4Label3, defaultDisplay),
//                                        //nameof(Resource.Lookup5) => ResourceDisplay(settings, def.Lookup5Visibility, def.Lookup5Label, def.Lookup5Label2, def.Lookup5Label3, defaultDisplay),
//                                        //nameof(Resource.Lookup5Id) => ResourceDisplay(settings, def.Lookup5Visibility, def.Lookup5Label, def.Lookup5Label2, def.Lookup5Label3, defaultDisplay),
//                                        nameof(Resource.Text1) => ResourceDisplay(settings, def.Text1Visibility, def.Text1Label, def.Text1Label2, def.Text1Label, defaultDisplay),
//                                        nameof(Resource.Text2) => ResourceDisplay(settings, def.Text2Visibility, def.Text2Label, def.Text2Label2, def.Text2Label, defaultDisplay),
//                                        // TODO: Add the rest
//                                        _ => display,
//                                    };
//                                }
//                                break;

//                            case nameof(Agent):
//                            case nameof(AgentForSave):
//                                {
//                                    if (!defs.Agents.TryGetValue(definitionId, out AgentDefinitionForClient def))
//                                    {
//                                        var msg = _localizer[$"Error_AgentDefinition0CouldNotBeFound"];
//                                        throw new BadRequestException(msg);
//                                    }

//                                    display = propInfo.Name switch
//                                    {
//                                        // TODO: Add the rest
//                                        _ => display,
//                                    };
//                                }
//                                break;

//                            case nameof(Document):
//                            case nameof(DocumentForSave):
//                                break;

//                            default:
//                                throw new InvalidOperationException($"Bug: Type '{entityType.Name}' is not definitioned, therefore a definitionId should not be supplied");

//                        }

//                        // Often the definition will hide some properties
//                        if (display == null)
//                        {
//                            // This property does not exist in this definition
//                            continue;
//                        }
//                    }

//                    //-////////////////////////////////////////
//                    // Validate
//                    //-////////////////////////////////////////

//                    // From validation attributes
//                    var validationAttributes = propInfo.GetCustomAttributes<ValidationAttribute>(inherit: true);

//                    // From definition
//                    bool isDefinitionRequired = false;

//                    // Default function
//                    Func<Entity, object, IEnumerable<ValidationResult>> validate = (Entity entity, object value) =>
//                    {
//                        var validationResults = new List<ValidationResult>();
//                        var ctx = new ValidationContext(entity, _serviceProvider, null)
//                        {
//                            DisplayName = display(),
//                            MemberName = propInfo.Name                            
//                        };

//                        foreach (var validationAtt in validationAttributes)
//                        {
//                            var validationResult = validationAtt.GetValidationResult(value, ctx);
//                            if (validationResult != null)
//                            {
//                                validationResults.Add(validationResult);
//                            }
//                        }

//                        // Definition required
//                        if (isDefinitionRequired)
//                        {
//                            validationResults.Add(new ValidationResult(_localizer[Services.Utilities.Constants.Error_TheField0IsRequired, display()]));
//                        }

//                        return validationResults;
//                    };

//                    //-////////////////////////////////////////
//                    // Type && Name
//                    //-////////////////////////////////////////
//                    var type = propInfo.PropertyType;
//                    var name = propInfo.Name;

//                    //-////////////////////////////////////////
//                    // Setter
//                    //-////////////////////////////////////////
//                    // (e, v) => e.Name = (string)v
//                    Action<Entity, object> setter;
//                    {
//                        var entityParam = Expression.Parameter(typeof(Entity), "e"); // e
//                        var valueParam = Expression.Parameter(typeof(object), "v"); // v
//                        var castEntity = Expression.Convert(entityParam, entityType); // (Account)e
//                        var propertyAccess = Expression.MakeMemberAccess(castEntity, propInfo); // ((Account)e).Name
//                        var convertedValue = Expression.Convert(valueParam, type); // (string)v
//                        var assignment = Expression.Assign(propertyAccess, convertedValue); // ((Account)e).Name = (string)v
//                        var lambdaExp = Expression.Lambda<Action<Entity, object>>(assignment, entityParam, valueParam); // (e, v) => ((Account)e).Name = (string)v

//                        setter = lambdaExp.Compile();
//                    }

//                    //-////////////////////////////////////////
//                    // Getter
//                    //-////////////////////////////////////////
//                    // (e) => e.Name;
//                    Func<Entity, object> getter;
//                    {
//                        var entityParam = Expression.Parameter(typeof(Entity), "e"); // e
//                        var castEntity = Expression.Convert(entityParam, entityType); // (Account)e
//                        var memberAccess = Expression.MakeMemberAccess(castEntity, propInfo); // ((Account)e).Name
//                        var castMemberAccess = Expression.Convert(memberAccess, typeof(object)); // (object)((Account)e).Name
//                        var lambdaExp = Expression.Lambda<Func<Entity, object>>(castMemberAccess, entityParam); // (e) => (object)((Account)e).Name

//                        getter = lambdaExp.Compile();
//                    }

//                    //-////////////////////////////////////////
//                    // Format
//                    //-////////////////////////////////////////
//                    Func<object, string> format = (v) => v.ToString(); // TODO

//                    // TODO

//                    //-////////////////////////////////////////
//                    // Parse
//                    //-////////////////////////////////////////
//                    Func<string, object> parse = (v) => v; // TODO

//                    // TODO
//                    // Functional decimal
//                    // Other decimal
//                    // Choice list
//                    // DateTime & DateTimeOffset
//                    // String and int
//                    // Boolean
//                    // Navigation
//                    // List
//                    // Serial


//                    // Add property descriptor
//                    PropertyDescriptor propDesc;
//                    if (propInfo.PropertyType.IsList())
//                    {
//                        //-////////////////////////////////////////
//                        // propEntityDefinitionId
//                        //-////////////////////////////////////////
//                        string propEntityDefinitionId = null; // TODO: Get from definition

//                        //-////////////////////////////////////////
//                        // ForeignKeyDesc
//                        //-////////////////////////////////////////
//                        var foreignKeyName = propInfo.GetCustomAttribute<ForeignKeyAttribute>()?.Name;
//                        if (string.IsNullOrWhiteSpace(foreignKeyName))
//                        {
//                            // Developer mistake
//                            throw new InvalidOperationException($"Collection property {propInfo.Name} on type {entityType.Name} is not adorned with the associated foreign key");
//                        }

//                        //-////////////////////////////////////////
//                        // getEntityDesc
//                        //-////////////////////////////////////////
//                        Type collectionType = propInfo.PropertyType.GetGenericArguments().SingleOrDefault();
//                        Func<EntityDescriptor> getCollectionEntityDescriptor = () => GetDescriptor(collectionType, propEntityDefinitionId, tenantId);

//                        // Collection
//                        propDesc = new CollectionPropertyDescriptor(
//                            type, name, display, validate, setter, getter, format,
//                            parse, foreignKeyName, getCollectionEntityDescriptor);

//                    }
//                    else if (propInfo.PropertyType.IsSubclassOf(typeof(Entity)))
//                    {
//                        //-////////////////////////////////////////
//                        // propEntityDefinitionId
//                        //-////////////////////////////////////////
//                        string propEntityDefinitionId = null; // TODO: Get from definition

//                        //-////////////////////////////////////////
//                        // IsParent
//                        //-////////////////////////////////////////
//                        bool isParent = propInfo.Name == "Parent" && entityType.GetProperty("Node")?.PropertyType == typeof(HierarchyId);

//                        //-////////////////////////////////////////
//                        // ForeignKeyDesc
//                        //-////////////////////////////////////////
//                        var fkName = propInfo.GetCustomAttribute<ForeignKeyAttribute>()?.Name;
//                        if (string.IsNullOrWhiteSpace(fkName))
//                        {
//                            // Developer mistake
//                            throw new InvalidOperationException($"Navigation property {propInfo.Name} on type {entityType.Name} is not adorned with the associated foreign key");
//                        }

//                        if(!properties.TryGetValue(fkName, out PropertyDescriptor foreignKeyDesc))
//                        {
//                            // Developer mistake
//                            throw new InvalidOperationException($"Navigation property {propInfo.Name} on type {entityType.Name} is adorned with a foreign key that doesn't exist");
//                        }

//                        //-////////////////////////////////////////
//                        // getEntityDesc
//                        //-////////////////////////////////////////
//                        Func<EntityDescriptor> getEntityDesc = () => GetDescriptor(propInfo.PropertyType, propEntityDefinitionId, tenantId);

//                        // Navigation
//                        propDesc = new NavigationPropertyDescriptor(
//                            type, name, display, validate, setter, getter, format,
//                            parse, propEntityDefinitionId, isParent, foreignKeyDesc, getEntityDesc);
//                    }
//                    else
//                    {
//                        // Simple
//                        propDesc = new PropertyDescriptor(type, name, display, validate, setter, getter, format, parse);
//                    }

//                    properties.Add(propInfo.Name, propDesc);
//                    propertyList.Add(propDesc);
//                }


//                var entityDesc = new EntityDescriptor(entityType, definitionId, create, createList, propertyList);
//                return entityDesc;
//            });
//        }

//        private Func<string> ResourceDisplay(SettingsForClient settings, string visibility, string label, string label2, string label3, Func<string> defaultDisplay)
//        {
//            if (visibility == null)
//            {
//                return null;
//            }
//            else
//            {
//                return () => settings.Localize(label, label2, label3) ?? defaultDisplay();
//            }
//        }

//        //class CacheEntry
//        //{
//        //    public DefinitionsForClient Definitions { get; }
//        //    public SettingsForClient Settings { get; }
//        //    public EntityDescriptor EntityDescriptor { get; }

//        //    public CacheEntry(DefinitionsForClient defs, SettingsForClient settings, EntityDescriptor entityDesc)
//        //    {
//        //        Definitions = defs;
//        //        Settings = settings;
//        //        EntityDescriptor = entityDesc;
//        //    }
//        //}

//        struct CacheKey
//        {
//            public int TenantId { get; set; }
//            public Type EntityType { get; set; }
//            public string DefinitionId { get; set; }
//            public SettingsForClient Settings { get; set; }
//            public DefinitionsForClient Definitions { get; set; }
//        }
//    }

//    public class EntityDescriptor
//    {
//        private readonly Func<Entity> _create;
//        private readonly Func<IList> _createList;

//        public Type Type { get; }

//        public KeyType KeyType { get; }

//        public string DefinitionId { get; }

//        public IReadOnlyDictionary<string, PropertyDescriptor> _propertiesDic { get; }

//        public IEnumerable<PropertyDescriptor> Properties { get; }

//        public IEnumerable<PropertyDescriptor> SimpleProperties { get; }

//        public IEnumerable<NavigationPropertyDescriptor> NavigationProperties { get; }

//        public IEnumerable<CollectionPropertyDescriptor> CollectionProperties { get; }

//        public Entity Create() => _create();

//        public IList CreateList() => _createList();

//        public bool HasId => _propertiesDic.ContainsKey("Id");

//        public EntityDescriptor(
//            Type type,
//            string definitionId,
//            Func<Entity> create,
//            Func<IList> createList,
//            IEnumerable<PropertyDescriptor> properties)
//        {
//            Type = type ?? throw new ArgumentNullException(nameof(type));
//            DefinitionId = definitionId;
//            _create = create ?? throw new ArgumentNullException(nameof(create));
//            _createList = createList ?? throw new ArgumentNullException(nameof(create));
//            Properties = properties ?? throw new ArgumentNullException(nameof(properties));
//            SimpleProperties = properties.Where(e => e.GetType() == typeof(PropertyDescriptor));
//            NavigationProperties = properties.OfType<NavigationPropertyDescriptor>();
//            CollectionProperties = properties.OfType<CollectionPropertyDescriptor>();
//            _propertiesDic = properties.ToDictionary(p => p.Name);

//            // Set key type
//            var prop = Property("Id");
//            if (prop == null)
//            {
//                KeyType = KeyType.None;
//            }
//            else if ((Nullable.GetUnderlyingType(prop.Type) ?? prop.Type) == typeof(int))
//            {
//                KeyType = KeyType.Int;
//            }
//            else if (prop.Type == typeof(string))
//            {
//                KeyType = KeyType.String;
//            }
//            else
//            {
//                throw new InvalidOperationException("Only int and string Ids are permitted");
//            }
//        }

//        public string Display()
//        {
//            if (DefinitionId == null)
//            {
//                return Type.Name;
//            }
//            else
//            {
//                return $"{Type.Name}/{DefinitionId}";
//            }
//        }

//        public bool HasProperty(string propName)
//        {
//            return _propertiesDic.ContainsKey(propName);
//        }

//        public PropertyDescriptor Property(string propName)
//        {
//            _propertiesDic.TryGetValue(propName, out PropertyDescriptor result);
//            return result;
//        }

//        public NavigationPropertyDescriptor NavigationProperty(string propName)
//        {
//            _propertiesDic.TryGetValue(propName, out PropertyDescriptor result);
//            return result as NavigationPropertyDescriptor;
//        }

//        public CollectionPropertyDescriptor CollectionProperty(string propName)
//        {
//            _propertiesDic.TryGetValue(propName, out PropertyDescriptor result);
//            return result as CollectionPropertyDescriptor;
//        }
//    }

//    public class PropertyDescriptor
//    {
//        private readonly Func<string> _display;
//        private readonly Func<Entity, object, IEnumerable<ValidationResult>> _validate;
//        private readonly Action<Entity, object> _setter;
//        private readonly Func<Entity, object> _getter;
//        private readonly Func<object, string> _format;
//        private readonly Func<string, object> _parse;

//        public Type Type { get; }

//        public string Name { get; }

//        public string Display() => _display();

//        public IEnumerable<ValidationResult> Validate(Entity entity, object value) => _validate(entity, value);

//        public void SetValue(Entity entity, object value) => _setter(entity, value);

//        public object GetValue(Entity entity) => _getter(entity);

//        public string Format(object value) => _format(value);

//        public object Parse(string stringValue) => _parse(stringValue);

//        public bool IsHierarchyId => Type == typeof(HierarchyId);

//        public PropertyDescriptor(
//            Type type,
//            string name,
//            Func<string> display,
//            Func<Entity, object, IEnumerable<ValidationResult>> validate,
//            Action<Entity, object> setter,
//            Func<Entity, object> getter,
//            Func<object, string> format,
//            Func<string, object> parse
//            )
//        {
//            Type = type ?? throw new ArgumentNullException(nameof(type));
//            Name = name ?? throw new ArgumentNullException(nameof(name));
//            _display = display ?? throw new ArgumentNullException(nameof(display));
//            _validate = validate ?? throw new ArgumentNullException(nameof(validate));
//            _setter = setter ?? throw new ArgumentNullException(nameof(setter));
//            _getter = getter ?? throw new ArgumentNullException(nameof(getter));
//            _format = format ?? throw new ArgumentNullException(nameof(format));
//            _parse = parse ?? throw new ArgumentNullException(nameof(parse));
//        }

//        /// <summary>
//        /// For navigation properties, returns the entity descriptor of the property type.
//        /// For collection properties, returns the entity descriptor of the collection's entity type.
//        /// For simple properties, throws an exception
//        /// </summary>
//        public EntityDescriptor GetEntityDescriptor()
//        {
//            if (this is NavigationPropertyDescriptor navProp)
//            {
//                return navProp.EntityDescriptor;
//            }
//            else if (this is CollectionPropertyDescriptor collProp)
//            {
//                return collProp.CollectionEntityDescriptor;
//            }
//            else
//            {
//                // Developer mistake
//                throw new InvalidOperationException($"Bug: Simple property {Name} is used like a navigation property");
//            }
//        }
//    }

//    public class NavigationPropertyDescriptor : PropertyDescriptor
//    {
//        private EntityDescriptor _entityDescriptor; // Caching
//        private readonly Func<EntityDescriptor> _getEntityDescriptor;

//        public string DefinitionId { get; }

//        public bool IsParent { get; }

//        public PropertyDescriptor ForeignKey { get; }
//        public EntityDescriptor EntityDescriptor => _entityDescriptor ??= _getEntityDescriptor();

//        public NavigationPropertyDescriptor(
//            Type type,
//            string name,
//            Func<string> display,
//            Func<Entity, object, IEnumerable<ValidationResult>> validate,
//            Action<Entity, object> setter,
//            Func<Entity, object> getter,
//            Func<object, string> format,
//            Func<string, object> parse,
//            string definitionId,
//            bool isParent,
//            PropertyDescriptor foreignKey,
//            Func<EntityDescriptor> getEntityDescriptor) : base(type, name, display, validate, setter, getter, format, parse)
//        {
//            _getEntityDescriptor = getEntityDescriptor;
//            IsParent = isParent;
//            DefinitionId = definitionId;
//            ForeignKey = foreignKey;
//        }
//    }

//    public class CollectionPropertyDescriptor : PropertyDescriptor
//    {
//        private readonly Func<EntityDescriptor> _getCollectionEntityDescriptor;

//        public string ForeignKeyName { get; set; }

//        private EntityDescriptor _collectionEntityDescriptor; // Caching
//        public EntityDescriptor CollectionEntityDescriptor => _collectionEntityDescriptor ??= _getCollectionEntityDescriptor();

//        public CollectionPropertyDescriptor(
//            Type type,
//            string name,
//            Func<string> display,
//            Func<Entity, object, IEnumerable<ValidationResult>> validate,
//            Action<Entity, object> setter,
//            Func<Entity, object> getter,
//            Func<object, string> format,
//            Func<string, object> parse,
//            string foreignKeyName,
//            Func<EntityDescriptor> getCollectionEntityDescriptor) : base(type, name, display, validate, setter, getter, format, parse)
//        {
//            ForeignKeyName = foreignKeyName;
//            _getCollectionEntityDescriptor = getCollectionEntityDescriptor;
//        }
//    }
//}
