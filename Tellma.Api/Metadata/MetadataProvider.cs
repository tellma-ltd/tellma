using Microsoft.Extensions.Localization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Tellma.Model.Common;

namespace Tellma.Api.Metadata
{
    /// <summary>
    /// Provides high level metadata about entity types that may rely on DefinitionId.
    /// </summary>
    public partial class MetadataProvider
    {
        /// <summary>
        /// Caches all the results of <see cref="GetMetadata(int?, Type, string)"/>
        /// </summary>
        private static readonly ConcurrentDictionary<CacheKey, CacheEntry> _cache = new();
        private static readonly NullMetadataOverridesProvider nullOverrides = new();

        private readonly IStringLocalizer<Strings> _localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataProvider"/> class. 
        /// This class is typically resolved using a dependency injection container.
        /// </summary>
        public MetadataProvider(IStringLocalizer<Strings> localizer)
        {
            _localizer = localizer;
        }

        /// <summary>
        /// Returns the <see cref="TypeMetadata"/> associated with a certain entity type of a certain definition Id in a certain tenantId (if any).
        /// The result is calculated once and cached forever until settings or definitions in that particular tenant have been updated.
        /// </summary>
        /// <param name="tenantId">The tenant Id from which the definitions are retrieved. NULL for admin.</param>
        /// <param name="entityType">The type to retrieve the metadata of.</param>
        /// <param name="definitionId">The definition ID to calculate the metadata based on.</param>
        /// <returns>The <see cref="TypeMetadata"/> associated with the entity type and definition Id in a certain tenantId.</returns>
        public TypeMetadata GetMetadata(int? tenantId, Type entityType, int? definitionId, IMetadataOverridesProvider overrides)
        {
            overrides ??= nullOverrides;

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
                TypeDescriptor typeDesc = TypeDescriptor.Get(cacheKey.EntityType);

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
                        string name = displayAtt.Name;
                        display = () => _localizer[name];
                    }

                    // e.g. "Decimal 1 Label"
                    var labelAtt = propInfo.GetCustomAttribute<DefinitionLabelDisplayAttribute>(inherit: true);
                    if (labelAtt != null)
                    {
                        string name = labelAtt.Name;
                        display = () => _localizer["Field0Label", _localizer[name]];
                    }

                    // e.g. "Currency Visibility"
                    var visibilityDisplayAtt = propInfo.GetCustomAttribute<VisibilityDisplayAttribute>(inherit: true);
                    if (visibilityDisplayAtt != null)
                    {
                        string name = visibilityDisplayAtt.Name;
                        display = () => _localizer["Field0Visibility", _localizer[name]];
                    }

                    // e.g. "Default Currency"
                    var defaultDisplayAtt = propInfo.GetCustomAttribute<DefaultDisplayAttribute>(inherit: true);
                    if (defaultDisplayAtt != null)
                    {
                        string name = defaultDisplayAtt.Name;
                        display = () => _localizer["Field0Default", _localizer[name]];
                    }

                    // e.g. "Lookup 1 Definition"
                    var ddDisplayAtt = propInfo.GetCustomAttribute<DefinitionDefinitionDisplayAttribute>(inherit: true);
                    if (ddDisplayAtt != null)
                    {
                        string name = ddDisplayAtt.Name;
                        display = () => _localizer["Field0Definition", _localizer[name]];
                    }

                    var isCommonAtt = propInfo.GetCustomAttribute<IsCommonDisplayAttribute>(inherit: true);
                    if (isCommonAtt != null)
                    {
                        string name = isCommonAtt.Name;
                        display = () => _localizer[name]; // Overrides know this will be the name of the original property
                    }

                    #endregion

                    #region Definition Override

                    // This is updated from defition, and is used later in the validation
                    bool isDefinitionRequired = false;
                    int? propDefinitionId = null;

                    // Get the definition override if any
                    PropertyMetadataOverrides propOverride = overrides.PropertyOverrides(typeDesc, definitionId, propDesc, display);
                    if (propOverride != null)
                    {
                        display = propOverride.Display;
                        isDefinitionRequired = propOverride.IsRequired;
                        propDefinitionId = propOverride.DefinitionId;
                    };

                    // Often the definition will hide some properties
                    if (display == null)
                    {
                        // This property does not exist in this definition
                        continue;
                    }
                    else if (isCommonAtt != null)
                    {
                        var originalDisplay = display;
                        display = () => _localizer["Field0IsCommon", originalDisplay()];
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
                        var ctx = new ValidationContext(entity)
                        {
                            DisplayName = displayName,
                            MemberName = propInfo.Name,
                        };

                        foreach (var validationAtt in validationAttributes)
                        {
                            if (validationAtt is RequiredAttribute)
                            {
                                // We use this attribute only to mean that the database column is NOT NULL, not for validation
                                continue;
                            }

                            var validationResult = validationAtt.GetValidationResult(value, ctx);
                            if (validationResult != null)
                            {
                                // Localize the error messages of these attributes
                                if (validationAtt is ValidateRequiredAttribute)
                                {
                                    // We use this one to mean that the property is required in the API
                                    string msg = _localizer[ErrorMessages.Error_Field0IsRequired, displayName];
                                    validationResult = new ValidationResult(msg);
                                }
                                else if (validationAtt is StringLengthAttribute strLengthAtt)
                                {
                                    string msgName = strLengthAtt.MinimumLength == 0 ?
                                        ErrorMessages.Error_Field0LengthMaximumOf1 :
                                        ErrorMessages.Error_Field0LengthMaximumOf1MinimumOf2;

                                    string msg = _localizer[msgName, displayName, strLengthAtt.MaximumLength, strLengthAtt.MinimumLength];
                                    validationResult = new ValidationResult(msg);
                                }
                                else if (validationAtt is EmailAddressAttribute)
                                {
                                    string msg = _localizer[ErrorMessages.Error_Field0IsNotValidEmail, displayName];
                                    validationResult = new ValidationResult(msg);
                                }
                                else if (validationAtt is PhoneAttribute)
                                {
                                    string msg = _localizer[ErrorMessages.Error_Field0IsNotValidPhone, displayName];
                                    validationResult = new ValidationResult(msg);
                                }

                                validationResults.Add(validationResult);
                            }
                        }

                        // The entity definition specifies that this is required
                        if (isDefinitionRequired && value == null)
                        {
                            validationResults.Add(new ValidationResult(_localizer[ErrorMessages.Error_Field0IsRequired, display()]));
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
                        TypeMetadata getCollectionTypeMetadata() => GetMetadata(tenantId, collectionType, propDefinitionId, overrides);

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

                        TypeMetadata getTypeMetadata() => GetMetadata(tenantId, propInfo.PropertyType, propDefinitionId, overrides);

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
                            else if (propDesc.Type == typeof(short?))
                            {
                                format = (obj) => obj?.ToString();
                                parse = (str) =>
                                {
                                    if (string.IsNullOrWhiteSpace(str))
                                    {
                                        return null;
                                    }
                                    else if (short.TryParse(str, out short result))
                                    {
                                        return result;
                                    }
                                    else
                                    {
                                        throw new ParseException(_localizer["Error_Value0IsNotAValid1Example2", str, _localizer["Short"], format(5)]);
                                    }
                                };
                            }
                            else if (propDesc.Type == typeof(byte?))
                            {
                                format = (obj) => obj?.ToString();
                                parse = (str) =>
                                {
                                    if (string.IsNullOrWhiteSpace(str))
                                    {
                                        return null;
                                    }
                                    else if (byte.TryParse(str, out byte result))
                                    {
                                        return result;
                                    }
                                    else
                                    {
                                        throw new ParseException(_localizer["Error_Value0IsNotAValid1Example2", str, _localizer["Byte"], format(50)]);
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
                                        throw new ParseException(_localizer[ErrorMessages.Error_Field0IsRequired, display()]);
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
                                    else if (double.TryParse(str, out double doubleResult))
                                    {
                                        return (decimal) doubleResult;
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
                var entityDisplayAtt = entityType.GetCustomAttribute<DisplayAttribute>(inherit: true);
                if (entityDisplayAtt != null)
                {
                    if (!string.IsNullOrWhiteSpace(entityDisplayAtt.Name))
                    {
                        singularDisplay = () => _localizer[entityDisplayAtt.Name];
                    }

                    if (!string.IsNullOrWhiteSpace(entityDisplayAtt.GroupName))
                    {
                        pluralDisplay = () => _localizer[entityDisplayAtt.GroupName];
                    }
                }

                // Entity Overrides
                EntityMetadataOverrides entityOverride = overrides.EntityOverrides(typeDesc, definitionId, singularDisplay, pluralDisplay);
                if (entityOverride != null)
                {
                    singularDisplay = entityOverride.SingularDisplay;
                    pluralDisplay = entityOverride.PluralDisplay;
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
                    OverridesProvider = overrides
                };
            });

            // This ensures that the result is refreshed when there are new settings or new definitions
            if (result.OverridesProvider != overrides)
            {
                // If the metadata are based on the wrong settings or definitions, clear and try again
                _cache.TryRemove(cacheKey, out _);
                return GetMetadata(tenantId, entityType, definitionId, overrides);
            }
            else
            {
                return result.Metadata;
            }
        }

        /// <summary>
        /// The key of the dictionary cache.
        /// </summary>
        private struct CacheKey
        {
            public int? TenantId { get; set; }
            public Type EntityType { get; set; }
            public int? DefinitionId { get; set; }
        }

        /// <summary>
        /// The entries stored in the dictionary cache.
        /// <para/>
        /// Upon retrieval from the cache the definitions and settings in the entry are verified
        /// to be the same as the current ones otherwise the entry is discarded and a new one is computed.
        /// </summary>
        private struct CacheEntry
        {
            public IMetadataOverridesProvider OverridesProvider { get; set; }
            public TypeMetadata Metadata { get; set; }
        }
    }
}
