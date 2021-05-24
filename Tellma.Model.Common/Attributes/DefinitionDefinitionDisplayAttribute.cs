using System;

namespace Tellma.Model.Common
{
    /// <summary>
    /// Adorns definition properties that store the display of entity properties, e.g. Lookup1Definition
    /// The name refers to the display name of the entity property, e.g. "Relation_Currency". The metadata
    /// provider automatically handles this by using the Field0Visibility key and passing the localization
    /// of name as a parameter.
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Property)]
    public class DefinitionDefinitionDisplayAttribute : Attribute
    {
        /// <summary>
        /// The display name of the original entity property.
        /// </summary>
        public string Name { get; set; }
    }
}
