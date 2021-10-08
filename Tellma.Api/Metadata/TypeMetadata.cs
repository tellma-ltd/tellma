using System;
using System.Collections.Generic;
using System.Linq;
using Tellma.Model.Common;

namespace Tellma.Api.Metadata
{
    /// <summary>
    /// Wrapper around <see cref="TypeDescriptor"/>. It provides additional metadata about a certain entity type that may be influenced
    /// by the entity definition. For example the definition may hide certain properties or change their labels.
    /// </summary>
    public class TypeMetadata
    {
        private readonly Func<string> _singularDisplay;
        private readonly Func<string> _pluralDisplay;
        private readonly PropertyMetadata _userKeyProp;
        private readonly IReadOnlyDictionary<string, PropertyMetadata> _propertiesDic;

        /// <summary>
        /// The <see cref="TypeDescriptor"/> wrapped by this <see cref="TypeMetadata"/>.
        /// </summary>
        public TypeDescriptor Descriptor { get; }

        /// <summary>
        /// The definition Id that this <see cref="TypeMetadata"/> is based on.
        /// </summary>
        public int? DefinitionId { get; }

        /// <summary>
        /// Properties with simple values such as strings, ints and decimals.
        /// </summary>
        public IEnumerable<PropertyMetadata> SimpleProperties { get; }

        /// <summary>
        /// Navigation properties that each point to another <see cref="Entity"/>.
        /// </summary>
        public IEnumerable<NavigationPropertyMetadata> NavigationProperties { get; }

        /// <summary>
        /// Collection navigation properties that each point to a <see cref="List{T}"/> of other entities.
        /// </summary>
        public IEnumerable<CollectionPropertyMetadata> CollectionProperties { get; }

        /// <summary>
        /// Returns the default user key of this entity type.
        /// <para/>
        /// For import and export purposes, the Id property is not user friendly for referring to 
        /// entities, so a "user key" property is used instead, such as Code or Name.
        /// User key properties are always of type string or int.
        /// </summary>
        public PropertyMetadata SuggestedUserKeyProperty => _userKeyProp ?? throw new InvalidOperationException($"Bug: Could not auto-determine the user key property for type {SingularDisplay()}");

        public string _key = Guid.NewGuid().ToString("D").Substring(0, 5);

        /// <summary>
        /// The display label of a single such entity, computed based on the current thread culture.
        /// For example: "Bank Account"
        /// </summary>
        public string SingularDisplay() => _singularDisplay();

        /// <summary>
        /// The display label of multiple such entities, computed based on the current thread culture.
        /// For example: "Bank Accounts"
        /// </summary>
        public string PluralDisplay() => _pluralDisplay();

        /// <summary>
        /// Constructor
        /// </summary>
        public TypeMetadata(
            TypeDescriptor desc,
            int? definitionId,
            Func<string> singularDisplay,
            Func<string> pluralDisplay,
            PropertyMetadata userKeyProp,
            IEnumerable<PropertyMetadata> properties)
        {

            // From descriptor
            Descriptor = desc;

            DefinitionId = definitionId;
            _singularDisplay = singularDisplay ?? throw new ArgumentNullException(nameof(singularDisplay));
            _pluralDisplay = pluralDisplay ?? throw new ArgumentNullException(nameof(pluralDisplay));
            _userKeyProp = userKeyProp;

            // Properties
            if (properties is null)
            {
                throw new ArgumentNullException(nameof(properties));
            }
            SimpleProperties = properties.Where(e => e.GetType() == typeof(PropertyMetadata));
            NavigationProperties = properties.OfType<NavigationPropertyMetadata>();
            CollectionProperties = properties.OfType<CollectionPropertyMetadata>();
            _propertiesDic = properties.ToDictionary(p => p.Descriptor.Name);
        }

        /// <summary>
        /// Returns the <see cref="PropertyMetadata"/> of the property with the given name,
        /// or false if no such property name exists
        /// </summary>
        public PropertyMetadata Property(string propName)
        {
            _propertiesDic.TryGetValue(propName, out PropertyMetadata result);
            return result;
        }

        /// <summary>
        /// Returns the <see cref="NavigationPropertyMetadata"/> of the navigation property with the given name,
        /// or false if no such navigation property name exists
        /// </summary>
        public NavigationPropertyMetadata NavigationProperty(string propName)
        {
            _propertiesDic.TryGetValue(propName, out PropertyMetadata result);
            return result as NavigationPropertyMetadata;
        }

        /// <summary>
        /// Returns the <see cref="NavigationPropertyMetadata"/> of the collection navigation property with the given name,
        /// or false if no such collection navigation property name exists
        /// </summary>
        public CollectionPropertyMetadata CollectionProperty(string propName)
        {
            _propertiesDic.TryGetValue(propName, out PropertyMetadata result);
            return result as CollectionPropertyMetadata;
        }
    }
}
