using Microsoft.Extensions.Localization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Tellma.Controllers.Dto;
using Tellma.Entities;
using Tellma.Entities.Descriptors;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    /// <summary>
    /// Provides high level metadata about entity types that may rely on DefinitionId
    /// </summary>
    public class MetadataProvider
    {
        private readonly IDefinitionsCache _definitionsCache;
        private readonly ISettingsCache _settingsCache;
        private readonly IStringLocalizer<Strings> _localizer;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Caches all the results of <see cref="GetMetadata(int?, Type, string)"/>
        /// </summary>
        private static readonly ConcurrentDictionary<CacheKey, CacheEntry> _cache = new ConcurrentDictionary<CacheKey, CacheEntry>();

        /// <summary>
        /// Constructor, this service is typically resolved with dependency injection
        /// </summary>
        public MetadataProvider(
            IDefinitionsCache definitionsCache,
            ISettingsCache settingsCache,
            IStringLocalizer<Strings> localizer,
            IServiceProvider serviceProvider)
        {
            _definitionsCache = definitionsCache;
            _settingsCache = settingsCache;
            _localizer = localizer;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Returns the <see cref="TypeMetadata"/> associated with a certain entity type of a certain definition Id in a certain tenantId (if any)
        /// The result is calculated once and cached forever until settings or definitions in that particular tenant have been updated
        /// </summary>
        /// <param name="tenantId">The tenant Id from which the definitions are retrieved. NULL for admin</param>
        /// <param name="entityType">The type to retrieve the metadata of</param>
        /// <param name="definitionId">The definition ID to calculate the metadata based on</param>
        /// <returns>The <see cref="TypeMetadata"/> associated with the entity type and definition Id in a certain tenantId</returns>
        public TypeMetadata GetMetadata(int? tenantId, Type entityType, int? definitionId = null)
        {
            // Get the settings
            SettingsForClient settings = tenantId == null ? null : _settingsCache.GetSettingsIfCached(tenantId.Value)?.Data ??
                    throw new InvalidOperationException($"Bug: The settings cache is empty for tenantId = {tenantId}");

            // Get the definitions
            DefinitionsForClient defs = null;
            if (definitionId != null)
            {
                tenantId = tenantId ?? throw new InvalidOperationException($"Bug: DefinitionId supplied without TenantId to retrieve metadata for {entityType?.Name}");
                defs = _definitionsCache.GetDefinitionsIfCached(tenantId.Value)?.Data ??
                    throw new InvalidOperationException($"Bug: The definitions cache is empty for tenantId = {tenantId}");
            }

            // Prepare the cache key
            var cacheKey = new CacheKey
            {
                TenantId = tenantId,
                EntityType = entityType,
                DefinitionId = definitionId,
            };

            // Retrieve the result in a thread-safe manner
            var result = _cache.GetOrAdd(cacheKey, (cacheKey) =>
            {
                // Prepare some stuff in advance
                Type entityType = cacheKey.EntityType;
                TypeDescriptor typeDesc = TypeDescriptor.Get(entityType);

                // Get the DefinitionForClient if the entity type supports it
                MasterDetailDefinitionForClient def = null;
                if (definitionId != null)
                {
                    switch (entityType.Name)
                    {
                        case nameof(Resource):
                        case nameof(ResourceForSave):
                            if (!defs.Resources.TryGetValue(definitionId.Value, out ResourceDefinitionForClient resourceDef))
                            {
                                var msg = _localizer[$"Error_ResourceDefinition0CouldNotBeFound", definitionId];
                                throw new BadRequestException(msg);
                            }
                            def = resourceDef;
                            break;

                        case nameof(Contract):
                        case nameof(ContractForSave):
                            if (!defs.Contracts.TryGetValue(definitionId.Value, out ContractDefinitionForClient contractDef))
                            {
                                var msg = _localizer[$"Error_ContractDefinition0CouldNotBeFound"];
                                throw new BadRequestException(msg);
                            }
                            def = contractDef;
                            break;

                        case nameof(Lookup):
                        case nameof(LookupForSave):
                            if (!defs.Lookups.TryGetValue(definitionId.Value, out LookupDefinitionForClient lookupDef))
                            {
                                var msg = _localizer[$"Error_LookupDefinition0CouldNotBeFound"];
                                throw new BadRequestException(msg);
                            }
                            def = lookupDef;
                            break;

                        case nameof(Document):
                        case nameof(DocumentForSave):
                            if (!defs.Documents.TryGetValue(definitionId.Value, out DocumentDefinitionForClient documentDef))
                            {
                                var msg = _localizer[$"Error_DocumentDefinition0CouldNotBeFound"];
                                throw new BadRequestException(msg);
                            }
                            def = documentDef;
                            break;

                        default:
                            throw new InvalidOperationException($"Bug: metadata for type {entityType.Name} is required with a definitionId {definitionId}, even though this type is not definitioned");
                    }
                }

                #region Properties

                var propertiesDic = new Dictionary<string, PropertyMetadata>();
                var properties = new List<PropertyMetadata>();

                // Loop over the properties and prepare the property metadatas
                foreach (var propDesc in typeDesc.Properties)
                {
                    #region PropertyInfo

                    var propInfo = propDesc.PropertyInfo;

                    #endregion

                    #region Display

                    // Default function
                    Func<string> display = () => propInfo.Name;

                    // Use DisplayAttribute if present
                    var displayAtt = propInfo.GetCustomAttribute<DisplayAttribute>();
                    if (displayAtt != null)
                    {
                        display = () => _localizer[displayAtt.Name];
                    }

                    // Use MultilingualDisplayAttribute if present
                    var multilingualAtt = propInfo.GetCustomAttribute<MultilingualDisplayAttribute>();
                    if (multilingualAtt != null)
                    {
                        string name = multilingualAtt.Name;
                        if (settings.SecondaryLanguageId != null || settings.TernaryLanguageId != null)
                        {
                            display = multilingualAtt.Language switch
                            {
                                Language.Primary => () => $"{_localizer[name]} ({settings.PrimaryLanguageSymbol})",
                                Language.Secondary => settings.SecondaryLanguageId == null ? (Func<string>)null : () => $"{_localizer[name]} ({settings.SecondaryLanguageSymbol})",
                                Language.Ternary => settings.TernaryLanguageId == null ? (Func<string>)null : () => $"{_localizer[name]} ({settings.TernaryLanguageSymbol})",
                                _ => throw new InvalidOperationException($"Unknown Language {multilingualAtt.Language}") // Future proofing
                            };
                        }
                        else
                        {
                            display = () => _localizer[name];
                        }
                    }

                    // e.g. "Currency Field Label"
                    var labelAtt = propInfo.GetCustomAttribute<DefinitionLabelDisplayAttribute>();
                    if (labelAtt != null)
                    {
                        string labelName = "Field0Label";
                        string name = labelAtt.Name;
                        if (settings.SecondaryLanguageId != null || settings.TernaryLanguageId != null)
                        {
                            display = labelAtt.Language switch
                            {
                                Language.Primary => () => $"{_localizer[labelName, _localizer[name]]} ({settings.PrimaryLanguageSymbol})",
                                Language.Secondary => settings.SecondaryLanguageId == null ? (Func<string>)null : () => $"{_localizer[labelName, _localizer[name]]} ({settings.SecondaryLanguageSymbol})",
                                Language.Ternary => settings.TernaryLanguageId == null ? (Func<string>)null : () => $"{_localizer[labelName, _localizer[name]]} ({settings.TernaryLanguageSymbol})",
                                _ => throw new InvalidOperationException($"Unknown Language {labelAtt.Language}") // Future proofing
                            };
                        }
                        else
                        {
                            display = () => _localizer[labelName, _localizer[name]];
                        }
                    }

                    // e.g. "Currency Field Visibility"
                    var visibilityDisplayAtt = propInfo.GetCustomAttribute<VisibilityDisplayAttribute>();
                    if (visibilityDisplayAtt != null)
                    {
                        string name = visibilityDisplayAtt.Name;
                        display = () => _localizer["Field0Visibility", _localizer[name]];
                    }

                    #endregion

                    #region Definition Override

                    // This is updated from defition, and is used later in the validation
                    bool isDefinitionRequired = false;

                    // Get the definition override if any
                    DefinitionPropOverrides defOverride = null;
                    if (def != null)
                    {
                        defOverride = entityType.Name switch
                        {
                            nameof(Resource) => ResourcePropertyOverrides(def as ResourceDefinitionForClient, settings, propInfo, display),
                            nameof(ResourceForSave) => ResourcePropertyOverrides(def as ResourceDefinitionForClient, settings, propInfo, display),

                            nameof(Contract) => ContractPropertyOverrides(def as ContractDefinitionForClient, settings, propInfo, display),
                            nameof(ContractForSave) => ContractPropertyOverrides(def as ContractDefinitionForClient, settings, propInfo, display),

                            nameof(Lookup) => LookupPropertyOverrides(def as LookupDefinitionForClient, settings, propInfo, display),
                            nameof(LookupForSave) => LookupPropertyOverrides(def as LookupDefinitionForClient, settings, propInfo, display),

                            nameof(Document) => DocumentPropertyOverrides(def as DocumentDefinitionForClient, settings, propInfo, display),
                            nameof(DocumentForSave) => DocumentPropertyOverrides(def as DocumentDefinitionForClient, settings, propInfo, display),

                            _ => throw new InvalidOperationException($"Bug: Unaccounted type in definition overrides {entityType.Name}")
                        };

                        display = defOverride.Display;
                        isDefinitionRequired = defOverride.IsRequired;
                    };

                    // Often the definition will hide some properties
                    if (display == null)
                    {
                        // This property does not exist in this definition
                        continue;
                    }

                    #endregion

                    #region Validate

                    // From validation attributes
                    var validationAttributes = propInfo.GetCustomAttributes<ValidationAttribute>(inherit: true);

                    // Default function
                    IEnumerable<ValidationResult> Validate(Entity entity, object value)
                    {
                        var validationResults = new List<ValidationResult>();
                        var displayName = display();
                        var ctx = new ValidationContext(entity, _serviceProvider, null)
                        {
                            DisplayName = displayName,
                            MemberName = propInfo.Name,
                        };

                        foreach (var validationAtt in validationAttributes)
                        {
                            var validationResult = validationAtt.GetValidationResult(value, ctx);
                            if (validationResult != null)
                            {
                                // Localize the error messages of these attributes
                                if (validationAtt is RequiredAttribute)
                                {
                                    string msg = _localizer[Constants.Error_Field0IsRequired, displayName];
                                    validationResult = new ValidationResult(msg);
                                }
                                else if (validationAtt is StringLengthAttribute strLengthAtt)
                                {
                                    string msgName = strLengthAtt.MinimumLength == 0 ?
                                        Constants.Error_Field0LengthMaximumOf1 :
                                        Constants.Error_Field0LengthMaximumOf1MinimumOf2;

                                    string msg = _localizer[msgName, displayName];
                                    validationResult = new ValidationResult(msg);
                                }
                                else if (validationAtt is EmailAddressAttribute)
                                {
                                    string msg = _localizer[Constants.Error_Field0IsNotValidEmail, displayName];
                                    validationResult = new ValidationResult(msg);
                                }

                                validationResults.Add(validationResult);
                            }
                        }

                        // The entity definition specifies that this is required
                        if (isDefinitionRequired && value == null)
                        {
                            validationResults.Add(new ValidationResult(_localizer[Constants.Error_Field0IsRequired, display()]));
                        }

                        return validationResults;
                    }

                    #endregion

                    // Add property descriptor
                    PropertyMetadata propMetadata;
                    if (propDesc is CollectionPropertyDescriptor collPropDesc)
                    {
                        #region getCollectionTypeMetadata

                        Type collectionType = propInfo.PropertyType.GetGenericArguments().SingleOrDefault();
                        Func<TypeMetadata> getCollectionTypeMetadata = () => GetMetadata(tenantId, collectionType, defOverride?.DefinitionId);

                        #endregion

                        // Collection
                        propMetadata = new CollectionPropertyMetadata(collPropDesc, display, Validate, getCollectionTypeMetadata);
                    }
                    else if (propDesc is NavigationPropertyDescriptor navPropDesc)
                    {
                        #region foreignKeyMetadata

                        var fkName = propInfo.GetCustomAttribute<ForeignKeyAttribute>()?.Name;
                        if (string.IsNullOrWhiteSpace(fkName))
                        {
                            // Developer mistake
                            throw new InvalidOperationException($"Navigation property {propInfo.Name} on type {entityType.Name} is not adorned with the associated foreign key");
                        }

                        if (!propertiesDic.TryGetValue(fkName, out PropertyMetadata foreignKeyMetadata))
                        {
                            // Developer mistake
                            throw new InvalidOperationException($"Navigation property {propInfo.Name} on type {entityType.Name} is adorned with a foreign key '{fkName}' which doesn't exist");
                        }

                        #endregion

                        #region getTypeMetadata

                        Func<TypeMetadata> getTypeMetadata = () => GetMetadata(tenantId, propInfo.PropertyType, defOverride?.DefinitionId);

                        #endregion

                        // Navigation
                        propMetadata = new NavigationPropertyMetadata(navPropDesc, display, Validate, foreignKeyMetadata, getTypeMetadata);
                    }
                    else
                    {
                        #region Format & Parse

                        // Maps property values to and from string
                        Func<object, string> format = null;
                        Func<string, object> parse = null;

                        var choiceListAtt = propInfo.GetCustomAttribute<ChoiceListAttribute>(inherit: true);
                        if (choiceListAtt != null)
                        {
                            var choices = choiceListAtt.Choices;
                            var displayNames = choiceListAtt.DisplayNames;

                            format = (obj) =>
                            {
                                if (obj == null)
                                {
                                    return null;
                                }

                                int index = -1;
                                for (int i = 0; i < choices.Length; i++)
                                {
                                    var choice = choices[i];
                                    if (obj.Equals(choice))
                                    {
                                        index = i;
                                        break;
                                    }
                                }

                                if (index >= 0)
                                {
                                    var displayName = displayNames[index];
                                    return _localizer[displayName];
                                }
                                else
                                {
                                    return obj.ToString();
                                }
                            };
                            parse = (str) =>
                            {
                                if (string.IsNullOrWhiteSpace(str))
                                {
                                    return null;
                                }

                                str = str.Trim();
                                int index = -1;
                                for (int i = 0; i < displayNames.Length; i++)
                                {
                                    var displayName = displayNames[i];
                                    if (_localizer[displayName] == str)
                                    {
                                        index = i;
                                        break;
                                    }
                                }

                                if (index >= 0)
                                {
                                    return choices[index];
                                }
                                else
                                {
                                    // Check if it's just wrong case, special error message
                                    for (int i = 0; i < displayNames.Length; i++)
                                    {
                                        var displayName = displayNames[i];
                                        var actual = _localizer[displayName];
                                        if (actual?.ToString()?.ToLower() == str)
                                        {
                                            index = i;
                                            throw new ParseException(_localizer["Error_Value0NotSupportedForField1DidYouMean2", str, display(), actual]);
                                        }
                                    }

                                    // Else error message mentioning full list of options
                                    var supportedValues = string.Join(", ", displayNames.Select(e => _localizer[e]));
                                    throw new ParseException(_localizer["Error_Value0NotSupportedForField1SupportedValuesAre2", str, display(), supportedValues]);
                                }
                            };
                        }
                        else
                        {
                            if (propDesc.Type == typeof(string))
                            {
                                format = (obj) => (string)obj;
                                parse = (str) => str;
                            }
                            else if (propDesc.Type == typeof(char?))
                            {
                                format = (obj) => obj?.ToString();
                                parse = (str) =>
                                {
                                    if (string.IsNullOrWhiteSpace(str))
                                    {
                                        return null;
                                    }
                                    else if (str.Length == 1)
                                    {
                                        return str.ToCharArray()[0];
                                    }
                                    else
                                    {
                                        throw new ParseException(_localizer["Error_Value0ForField1ShouldNotExceedOneChar", str, display()]);
                                    }
                                };
                            }
                            else if (propDesc.Type == typeof(int?))
                            {
                                format = (obj) => obj?.ToString();
                                parse = (str) =>
                                {
                                    if (string.IsNullOrWhiteSpace(str))
                                    {
                                        return null;
                                    }
                                    else if (int.TryParse(str, out int result))
                                    {
                                        return result;
                                    }
                                    else
                                    {
                                        throw new ParseException(_localizer["Error_Value0IsNotAValid1Example2", str, _localizer["Integer"], format(50)]);
                                    }
                                };
                            }
                            else if (propDesc.Type == typeof(int))
                            {
                                format = (obj) => obj?.ToString();
                                parse = (str) =>
                                {
                                    if (string.IsNullOrWhiteSpace(str))
                                    {
                                        throw new ParseException(_localizer[Constants.Error_Field0IsRequired, display()]);
                                    }
                                    else if (int.TryParse(str, out int result))
                                    {
                                        return result;
                                    }
                                    else
                                    {
                                        throw new ParseException(_localizer["Error_Value0IsNotAValid1Example2", str, _localizer["Integer"], format(50)]);
                                    }
                                };
                            }
                            else if (propDesc.Type == typeof(DateTimeOffset?))
                            {
                                format = (obj) => obj?.ToString();
                                parse = (str) =>
                                {
                                    if (string.IsNullOrWhiteSpace(str))
                                    {
                                        return null;
                                    }
                                    else if (DateTimeOffset.TryParse(str, out DateTimeOffset result))
                                    {
                                        return result;
                                    }
                                    else
                                    {
                                        throw new ParseException(_localizer["Error_Value0IsNotAValid1Example2", str, _localizer["DateTimeOffset"], format(DateTimeOffset.Now)]);
                                    }
                                };
                            }
                            else if (propDesc.Type == typeof(DateTime?))
                            {
                                // TODO: handle DateTime's that are meant to represent time as well
                                format = (obj) => obj == null ? null : string.Format("{0:yyyy-MM-dd}", obj);
                                parse = (str) =>
                                {
                                    if (string.IsNullOrWhiteSpace(str))
                                    {
                                        return null;
                                    }
                                    else if (DateTime.TryParse(str, out DateTime result))
                                    {
                                        return result;
                                    }
                                    else if (double.TryParse(str, out double d))
                                    {
                                        // Double indicates an OLE Automation date which typically comes from excel
                                        return DateTime.FromOADate(d);
                                    }
                                    else
                                    {
                                        throw new ParseException(_localizer["Error_Value0IsNotAValid1Example2", str, _localizer["DateTime"], format(DateTime.Today)]);
                                    }
                                };
                            }
                            else if (propDesc.Type == typeof(decimal?))
                            {
                                format = (obj) => obj?.ToString();
                                parse = (str) =>
                                {
                                    if (string.IsNullOrWhiteSpace(str))
                                    {
                                        return null;
                                    }
                                    else if (decimal.TryParse(str, out decimal result))
                                    {
                                        return result;
                                    }
                                    else
                                    {
                                        throw new ParseException(_localizer["Error_Value0IsNotAValid1Example2", str, _localizer["Decimal"], format(21502.75m)]);
                                    }
                                };
                            }
                            else if (propDesc.Type == typeof(double?))
                            {
                                format = (obj) => obj?.ToString();
                                parse = (str) =>
                                {
                                    if (string.IsNullOrWhiteSpace(str))
                                    {
                                        return null;
                                    }
                                    else if (double.TryParse(str, out double result))
                                    {
                                        return result;
                                    }
                                    else
                                    {
                                        throw new ParseException(_localizer["Error_Value0IsNotAValid1Example2", str, _localizer["Decimal"], format(12502.75)]);
                                    }
                                };
                            }
                            else if (propDesc.Type == typeof(bool?))
                            {
                                // TODO: Custom display for booleans
                                format = (obj) =>
                                {
                                    if (obj is null)
                                    {
                                        return null;
                                    }
                                    else if ((bool)obj)
                                    {
                                        return _localizer["Yes"];
                                    }
                                    else
                                    {
                                        return _localizer["No"];
                                    }
                                };

                                parse = (str) =>
                                {
                                    if (string.IsNullOrWhiteSpace(str))
                                    {
                                        return null;
                                    }
                                    else
                                    {
                                        var objString = str.ToString().ToLower();
                                        var yes = _localizer["Yes"].ToString().ToLower();
                                        var no = _localizer["No"].ToString().ToLower();

                                        if (objString == yes)
                                        {
                                            return true;
                                        }
                                        else if (objString == no)
                                        {
                                            return false;
                                        }
                                        else
                                        {
                                            throw new ParseException(_localizer["Error_Value0NotSupportedForField1SupportedValuesAre2", str, display(), $"{yes},{no}"]);
                                        }
                                    }
                                };
                            }
                        }

                        #endregion

                        // Simple
                        propMetadata = new PropertyMetadata(propDesc, display, Validate, format, parse);
                    }

                    propertiesDic.Add(propInfo.Name, propMetadata);
                    properties.Add(propMetadata);
                };

                #endregion

                #region Entity Display

                Func<string> singularDisplay = () => _localizer[entityType.Name];
                Func<string> pluralDisplay = () => _localizer[entityType.Name];
                var entityDisplayAtt = entityType.GetCustomAttribute<EntityDisplayAttribute>(inherit: true);
                if (entityDisplayAtt != null)
                {
                    if (!string.IsNullOrWhiteSpace(entityDisplayAtt.Singular))
                    {
                        singularDisplay = () => _localizer[entityDisplayAtt.Singular];
                    }

                    if (!string.IsNullOrWhiteSpace(entityDisplayAtt.Plural))
                    {
                        pluralDisplay = () => _localizer[entityDisplayAtt.Plural];
                    }
                }


                // Definition Override
                if (def != null)
                {
                    var defaultSingular = singularDisplay;
                    singularDisplay = () => settings.Localize(def.TitleSingular, def.TitleSingular2, def.TitleSingular3) ?? defaultSingular();

                    var defaultPlural = pluralDisplay;
                    pluralDisplay = () => settings.Localize(def.TitlePlural, def.TitlePlural2, def.TitlePlural3) ?? defaultPlural();
                }

                #endregion

                #region User Key

                PropertyMetadata userKeyProp = null;

                // First priority an explicit attribute
                var propWithUserKeyAttribute = properties.FirstOrDefault(p => p.Descriptor.PropertyInfo.GetCustomAttribute<UserKeyAttribute>() != null);
                if (propWithUserKeyAttribute != null)
                {
                    userKeyProp = propWithUserKeyAttribute;
                }
                // Second priority a string Id property
                else if (propertiesDic.TryGetValue("Id", out PropertyMetadata idProp) && idProp.Descriptor.Type == typeof(string))
                {
                    userKeyProp = idProp;
                }
                // Third priority a Code property
                else if (propertiesDic.TryGetValue("Code", out PropertyMetadata codeProp) && codeProp.Descriptor.Type == typeof(string))
                {
                    userKeyProp = codeProp;
                }
                // Fourth priority a Name property
                else if (propertiesDic.TryGetValue("Name", out PropertyMetadata nameProp) && nameProp.Descriptor.Type == typeof(string))
                {
                    userKeyProp = nameProp;
                }
                // Fifth priority a Label property
                else if (propertiesDic.TryGetValue("Label", out PropertyMetadata labelProp) && labelProp.Descriptor.Type == typeof(string))
                {
                    userKeyProp = labelProp;
                }

                #endregion

                var typeMetadata = new TypeMetadata(typeDesc, definitionId, singularDisplay, pluralDisplay, userKeyProp, properties);
                return new CacheEntry
                {
                    Metadata = typeMetadata,
                    Definitions = defs,
                    Settings = settings
                };
            });

            // This ensures that the result is refreshed when there are new settings or new definitions
            if (result.Definitions != defs || result.Settings != settings)
            {
                // If the metadata are based on the wrong settings or definitions, clear and try again
                _cache.TryRemove(cacheKey, out result);
                return GetMetadata(tenantId, entityType, definitionId);
            }
            else
            {
                return result.Metadata;
            }
        }

        #region Definition Override

        /// <summary>
        /// Specifies any overriding changes to a resource property metadata that stem from the definition. 
        /// In particular: the property display, whether it's visible or not, whether it's required or not, 
        /// and - if it's a navigation property - the target definitionId
        /// </summary>
        private static DefinitionPropOverrides ResourcePropertyOverrides(
            ResourceDefinitionForClient def, 
            SettingsForClient settings, 
            PropertyInfo propInfo,
            Func<string> display)
        {
            bool isRequired = false;
            
            switch (propInfo.Name)
            {
                case nameof(Resource.Description):
                case nameof(Resource.Description2):
                case nameof(Resource.Description3):
                    display = PropertyDisplay(def.DescriptionVisibility, display);
                    isRequired = def.DescriptionVisibility == Visibility.Required;
                    break;
                case nameof(Resource.Location):
                case nameof(Resource.LocationJson):
                case nameof(Resource.LocationWkb):
                    display = PropertyDisplay(def.LocationVisibility, display);
                    isRequired = def.LocationVisibility == Visibility.Required;
                    break;
                case nameof(Resource.ReorderLevel):
                    display = PropertyDisplay(def.ReorderLevelVisibility, display);
                    isRequired = def.ReorderLevelVisibility == Visibility.Required;
                    break;
                case nameof(Resource.EconomicOrderQuantity):
                    display = PropertyDisplay(def.EconomicOrderQuantityVisibility, display);
                    isRequired = def.EconomicOrderQuantityVisibility == Visibility.Required;
                    break;
                case nameof(Resource.ResidualMonetaryValue):
                    display = PropertyDisplay(def.ResidualMonetaryValueVisibility, display);
                    isRequired = def.ResidualMonetaryValueVisibility == Visibility.Required;
                    break;
                case nameof(Resource.ResidualValue):
                    display = PropertyDisplay(def.ResidualValueVisibility, display);
                    isRequired = def.ResidualValueVisibility == Visibility.Required;
                    break;
                case nameof(Resource.Identifier):
                    display = PropertyDisplay(settings, def.IdentifierVisibility, def.IdentifierLabel, def.IdentifierLabel2, def.IdentifierLabel3, display);
                    isRequired = def.IdentifierVisibility == Visibility.Required;
                    break;
                case nameof(Resource.MonetaryValue):
                    display = PropertyDisplay(settings, def.MonetaryValueVisibility, def.MonetaryValueLabel, def.MonetaryValueLabel2, def.MonetaryValueLabel3, display);
                    isRequired = def.MonetaryValueVisibility == Visibility.Required;
                    break;
                case nameof(Resource.AvailableSince):
                    display = PropertyDisplay(settings, def.AvailableSinceVisibility, def.AvailableSinceLabel, def.AvailableSinceLabel2, def.AvailableSinceLabel3, display);
                    isRequired = def.AvailableSinceVisibility == Visibility.Required;
                    break;
                case nameof(Resource.AvailableTill):
                    display = PropertyDisplay(settings, def.AvailableTillVisibility, def.AvailableTillLabel, def.AvailableTillLabel2, def.AvailableTillLabel3, display);
                    isRequired = def.AvailableTillVisibility == Visibility.Required;
                    break;
                case nameof(Resource.Decimal1):
                    display = PropertyDisplay(settings, def.Decimal1Visibility, def.Decimal1Label, def.Decimal1Label2, def.Decimal1Label3, display);
                    isRequired = def.Decimal1Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Decimal2):
                    display = PropertyDisplay(settings, def.Decimal2Visibility, def.Decimal2Label, def.Decimal2Label2, def.Decimal2Label3, display);
                    isRequired = def.Decimal2Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Int1):
                    display = PropertyDisplay(settings, def.Int1Visibility, def.Int1Label, def.Int1Label2, def.Int1Label3, display);
                    isRequired = def.Int1Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Int2):
                    display = PropertyDisplay(settings, def.Int2Visibility, def.Int2Label, def.Int2Label2, def.Int2Label3, display);
                    isRequired = def.Int2Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Text1):
                    display = PropertyDisplay(settings, def.Text1Visibility, def.Text1Label, def.Text1Label2, def.Text1Label3, display);
                    isRequired = def.Text1Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Text2):
                    display = PropertyDisplay(settings, def.Text2Visibility, def.Text2Label, def.Text2Label2, def.Text2Label3, display);
                    isRequired = def.Text2Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Currency):
                    display = PropertyDisplay(settings, def.CurrencyVisibility, def.CurrencyLabel, def.CurrencyLabel2, def.CurrencyLabel3, display);
                    isRequired = def.CurrencyVisibility == Visibility.Required;
                    break;
                case nameof(Resource.CurrencyId):
                    display = PropertyDisplay(settings, def.CurrencyVisibility, def.CurrencyLabel, def.CurrencyLabel2, def.CurrencyLabel3, display);
                    isRequired = def.CurrencyVisibility == Visibility.Required;
                    break;
                case nameof(Resource.ExpenseEntryType):
                    display = PropertyDisplay(def.ExpenseEntryTypeVisibility, display);
                    isRequired = def.ExpenseEntryTypeVisibility == Visibility.Required;
                    break;
                case nameof(Resource.ExpenseEntryTypeId):
                    display = PropertyDisplay(def.ExpenseEntryTypeVisibility, display);
                    isRequired = def.ExpenseEntryTypeVisibility == Visibility.Required;
                    break;
                case nameof(Resource.Center):
                    display = PropertyDisplay(def.CenterVisibility, display);
                    isRequired = def.CenterVisibility == Visibility.Required;
                    break;
                case nameof(Resource.CenterId):
                    display = PropertyDisplay(def.CenterVisibility, display);
                    isRequired = def.CenterVisibility == Visibility.Required;
                    break;
                case nameof(Resource.Lookup1):
                    display = PropertyDisplay(settings, def.Lookup1Visibility, def.Lookup1Label, def.Lookup1Label2, def.Lookup1Label3, display);
                    isRequired = def.Lookup1Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Lookup1Id):
                    display = PropertyDisplay(settings, def.Lookup1Visibility, def.Lookup1Label, def.Lookup1Label2, def.Lookup1Label3, display);
                    isRequired = def.Lookup1Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Lookup2):
                    display = PropertyDisplay(settings, def.Lookup2Visibility, def.Lookup2Label, def.Lookup2Label2, def.Lookup2Label3, display);
                    isRequired = def.Lookup2Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Lookup2Id):
                    display = PropertyDisplay(settings, def.Lookup2Visibility, def.Lookup2Label, def.Lookup2Label2, def.Lookup2Label3, display);
                    isRequired = def.Lookup2Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Lookup3):
                    display = PropertyDisplay(settings, def.Lookup3Visibility, def.Lookup3Label, def.Lookup3Label2, def.Lookup3Label3, display);
                    isRequired = def.Lookup3Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Lookup3Id):
                    display = PropertyDisplay(settings, def.Lookup3Visibility, def.Lookup3Label, def.Lookup3Label2, def.Lookup3Label3, display);
                    isRequired = def.Lookup3Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Lookup4):
                    display = PropertyDisplay(settings, def.Lookup4Visibility, def.Lookup4Label, def.Lookup4Label2, def.Lookup4Label3, display);
                    isRequired = def.Lookup4Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Lookup4Id):
                    display = PropertyDisplay(settings, def.Lookup4Visibility, def.Lookup4Label, def.Lookup4Label2, def.Lookup4Label3, display);
                    isRequired = def.Lookup4Visibility == Visibility.Required;
                    break;
                //case nameof(Resource.Lookup5):
                //    display = PropertyDisplay(settings, def.Lookup5Visibility, def.Lookup5Label, def.Lookup5Label2, def.Lookup5Label3, display);
                //    isRequired = def.Lookup5Visibility == Visibility.Required;
                //    break;
                //case nameof(Resource.Lookup5Id):
                //    display = PropertyDisplay(settings, def.Lookup5Visibility, def.Lookup5Label, def.Lookup5Label2, def.Lookup5Label3, display);
                //    isRequired = def.Lookup5Visibility == Visibility.Required;
                //    break;
            }

            int? targetDefId = propInfo.Name switch
            {
                nameof(Resource.Lookup1) => def.Lookup1DefinitionId,
                nameof(Resource.Lookup2) => def.Lookup2DefinitionId,
                nameof(Resource.Lookup3) => def.Lookup3DefinitionId,
                nameof(Resource.Lookup4) => def.Lookup4DefinitionId,
                //nameof(Resource.Lookup5) =>  def.Lookup5DefinitionId,
                _ => null,
            };

            return new DefinitionPropOverrides
            {
                Display = display,
                IsRequired = isRequired,
                DefinitionId = targetDefId,
            };
        }

        /// <summary>
        /// Specifies any overriding changes to a contract property metadata that stem from the definition. 
        /// In particular: the property display, whether it's visible or not, whether it's required or not, 
        /// and - if it's a navigation property - the target definitionId
        /// </summary>

        private static DefinitionPropOverrides ContractPropertyOverrides(
            ContractDefinitionForClient def,
            SettingsForClient _,
            PropertyInfo propInfo,
            Func<string> display)
        {
            bool isRequired = false;

            switch (propInfo.Name)
            {
                case nameof(Contract.AgentId):
                case nameof(Contract.Agent):
                    display = PropertyDisplay(def.AgentVisibility, display);
                    isRequired = def.AgentVisibility == Visibility.Required;
                    break;
                case nameof(Contract.CurrencyId):
                case nameof(Contract.Currency):
                    display = PropertyDisplay(def.CurrencyVisibility, display);
                    isRequired = def.CurrencyVisibility == Visibility.Required;
                    break;
                case nameof(Contract.TaxIdentificationNumber):
                    display = PropertyDisplay(def.TaxIdentificationNumberVisibility, display);
                    isRequired = def.TaxIdentificationNumberVisibility == Visibility.Required;
                    break;
                case nameof(Contract.StartDate):
                    display = PropertyDisplay(def.StartDateVisibility, display);
                    isRequired = def.StartDateVisibility == Visibility.Required;
                    break;
                case nameof(Contract.JobId):
                    display = PropertyDisplay(def.JobVisibility, display);
                    isRequired = def.JobVisibility == Visibility.Required;
                    break;
                case nameof(Contract.BankAccountNumber):
                    display = PropertyDisplay(def.BankAccountNumberVisibility, display);
                    isRequired = def.BankAccountNumberVisibility == Visibility.Required;
                    break;
                case nameof(Contract.User):
                    if (def.UserVisibility != null && def.AllowMultipleUsers)
                    {
                        display = null;
                        isRequired = false;
                    }
                    else
                    {
                        display = PropertyDisplay(def.UserVisibility, display);
                        isRequired = def.UserVisibility == Visibility.Required;
                    }
                    break;
            }

            return new DefinitionPropOverrides
            {
                Display = display,
                IsRequired = isRequired
            };
        }

        /// <summary>
        /// Specifies any overriding changes to a lookup property metadata that stem from the definition. 
        /// In particular: the property display, whether it's visible or not, whether it's required or not, 
        /// and - if it's a navigation property - the target definitionId
        /// </summary>
        private static DefinitionPropOverrides LookupPropertyOverrides(
            LookupDefinitionForClient _1,
            SettingsForClient _2,
            PropertyInfo _3,
            Func<string> display)
        {
            return new DefinitionPropOverrides
            {
                Display = display
            };
        }

        /// <summary>
        /// Specifies any overriding changes to a document property metadata that stem from the definition. 
        /// In particular: the property display, whether it's visible or not, whether it's required or not, 
        /// and - if it's a navigation property - the target definitionId
        /// </summary>
        private static DefinitionPropOverrides DocumentPropertyOverrides(
            DocumentDefinitionForClient def,
            SettingsForClient settings,
            PropertyInfo propInfo,
            Func<string> display)
        {
            bool isRequired = false;

            switch (propInfo.Name)
            {
                case nameof(Document.Memo):
                    display = PropertyDisplay(settings, def.MemoVisibility, def.MemoLabel, def.MemoLabel2, def.MemoLabel3, display);
                    isRequired = def.MemoVisibility == Visibility.Required;
                    break;
                case nameof(Document.PostingDate):
                    display = PropertyDisplay(settings, def.PostingDateVisibility, def.PostingDateLabel, def.PostingDateLabel2, def.PostingDateLabel3, display);
                    isRequired = def.PostingDateRequiredState == 0;
                    break;
                case nameof(Document.DebitContractId):
                    display = PropertyDisplay(settings, def.DebitContractVisibility, def.DebitContractLabel, def.DebitContractLabel2, def.DebitContractLabel3, display);
                    isRequired = def.DebitContractRequiredState == 0;
                    break;
                case nameof(Document.DebitContract):
                    display = PropertyDisplay(settings, def.DebitContractVisibility, def.DebitContractLabel, def.DebitContractLabel2, def.DebitContractLabel3, display);
                    isRequired = def.DebitContractRequiredState == 0;
                    break;
                case nameof(Document.CreditContractId):
                    display = PropertyDisplay(settings, def.CreditContractVisibility, def.CreditContractLabel, def.CreditContractLabel2, def.CreditContractLabel3, display);
                    isRequired = def.CreditContractRequiredState == 0;
                    break;
                case nameof(Document.CreditContract):
                    display = PropertyDisplay(settings, def.CreditContractVisibility, def.CreditContractLabel, def.CreditContractLabel2, def.CreditContractLabel3, display);
                    isRequired = def.CreditContractRequiredState == 0;
                    break;
                case nameof(Document.NotedContractId):
                    display = PropertyDisplay(settings, def.NotedContractVisibility, def.NotedContractLabel, def.NotedContractLabel2, def.NotedContractLabel3, display);
                    isRequired = def.NotedContractRequiredState == 0;
                    break;
                case nameof(Document.NotedContract):
                    display = PropertyDisplay(settings, def.NotedContractVisibility, def.NotedContractLabel, def.NotedContractLabel2, def.NotedContractLabel3, display);
                    isRequired = def.NotedContractRequiredState == 0;
                    break;
                case nameof(Document.Segment):
                    display = PropertyDisplay(settings, def.SegmentVisibility, def.SegmentLabel, def.SegmentLabel2, def.SegmentLabel3, display);
                    isRequired = def.SegmentRequiredState == 0;
                    break;
                case nameof(Document.SegmentId):
                    display = PropertyDisplay(settings, def.SegmentVisibility, def.SegmentLabel, def.SegmentLabel2, def.SegmentLabel3, display);
                    isRequired = def.SegmentRequiredState == 0;
                    break;
                case nameof(Document.Time1):
                    display = PropertyDisplay(settings, def.Time1Visibility, def.Time1Label, def.Time1Label2, def.Time1Label3, display);
                    isRequired = def.Time1RequiredState == 0;
                    break;
                case nameof(Document.Time2):
                    display = PropertyDisplay(settings, def.Time2Visibility, def.Time2Label, def.Time2Label2, def.Time2Label3, display);
                    isRequired = def.Time2RequiredState == 0;
                    break;
                case nameof(Document.Quantity):
                    display = PropertyDisplay(settings, def.QuantityVisibility, def.QuantityLabel, def.QuantityLabel2, def.QuantityLabel3, display);
                    isRequired = def.QuantityRequiredState == 0;
                    break;
                case nameof(Document.UnitId):
                    display = PropertyDisplay(settings, def.UnitVisibility, def.UnitLabel, def.UnitLabel2, def.UnitLabel3, display);
                    isRequired = def.UnitRequiredState == 0;
                    break;
                case nameof(Document.Unit):
                    display = PropertyDisplay(settings, def.UnitVisibility, def.UnitLabel, def.UnitLabel2, def.UnitLabel3, display);
                    isRequired = def.UnitRequiredState == 0;
                    break;
                case nameof(Document.CurrencyId):
                    display = PropertyDisplay(settings, def.CurrencyVisibility, def.CurrencyLabel, def.CurrencyLabel2, def.CurrencyLabel3, display);
                    isRequired = def.CurrencyRequiredState == 0;
                    break;
                case nameof(Document.Currency):
                    display = PropertyDisplay(settings, def.CurrencyVisibility, def.CurrencyLabel, def.CurrencyLabel2, def.CurrencyLabel3, display);
                    isRequired = def.CurrencyRequiredState == 0;
                    break;
            }

            int? targetDefId = propInfo.Name switch
            {
                nameof(Document.DebitContract) => def.DebitContractDefinitionIds.Count == 1 ? (int?)def.DebitContractDefinitionIds[0] : null,
                nameof(Document.CreditContract) => def.CreditContractDefinitionIds.Count == 1 ? (int?)def.CreditContractDefinitionIds[0] : null,
                nameof(Document.NotedContract) => def.NotedContractDefinitionIds.Count == 1 ? (int?)def.NotedContractDefinitionIds[0] : null,
                _ => null,
            };

            return new DefinitionPropOverrides
            {
                Display = display,
                IsRequired = isRequired,
                DefinitionId = targetDefId,
            };
        }

        /// <summary>
        /// Returns null if the visibility or null, returns the same display function otherwise
        /// </summary>
        private static Func<string> PropertyDisplay(
            string visibility,
            Func<string> defaultDisplay)
        {
            if (visibility == null)
            {
                return null;
            }
            else
            {
                return defaultDisplay;
            }
        }

        /// <summary>
        /// Returns null if the visibility is null, otherwise returns a new display function
        /// that reads the relies on the supplied labels, and falls back to the default function
        /// if the labels are null
        /// </summary>
        private static Func<string> PropertyDisplay(
            SettingsForClient settings, 
            string visibility, 
            string label, 
            string label2, 
            string label3, 
            Func<string> defaultDisplay)
        {
            if (visibility == null)
            {
                return null;
            }
            else
            {
                return () => settings.Localize(label, label2, label3) ?? defaultDisplay();
            }
        }

        /// <summary>
        /// Returns null if the visibility is false, otherwise returns a new display function
        /// that reads the relies on the supplied labels, and falls back to the default function
        /// if the labels are null
        /// </summary>
        private static Func<string> PropertyDisplay(
            SettingsForClient settings,
            bool isVisible,
            string label,
            string label2,
            string label3,
            Func<string> defaultDisplay)
        {
            if (isVisible)
            {
                return () => settings.Localize(label, label2, label3) ?? defaultDisplay();
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Helper classes

        /// <summary>
        /// Carries information about an entity property that override its default metadata
        /// </summary>
        private class DefinitionPropOverrides
        {
            /// <summary>
            /// Overrides the default display function of the property, or if null hides the property entirely
            /// </summary>
            public Func<string> Display { get; set; }

            /// <summary>
            /// Indicates that the entity property is required in the entity definition
            /// </summary>
            public bool IsRequired { get; set; }

            /// <summary>
            /// For navigation properties, this specifies the target definition Id
            /// </summary>
            public int? DefinitionId { get; set; }
        }

        /// <summary>
        /// The key of the dictionary cache
        /// </summary>
        private struct CacheKey
        {
            public int? TenantId { get; set; }
            public Type EntityType { get; set; }
            public int? DefinitionId { get; set; }
        }

        /// <summary>
        /// The entries stored in the dictionary cache.
        /// Upon retrieval from the cache the definitions and settings in the entry are verified
        /// to be the same as the current ones otherwise the entry is discarded and a new one is computed
        /// </summary>
        private struct CacheEntry
        {
            public DefinitionsForClient Definitions { get; set; }
            public SettingsForClient Settings { get; set; }
            public TypeMetadata Metadata { get; set; }
        }

        #endregion
    }
}
