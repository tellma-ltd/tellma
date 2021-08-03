using System;

namespace Tellma.Model.Common
{
    /// <summary>
    /// Adorns definition properties that store the default value of entity properties, e.g. DefaultCurrency
    /// The name refers to the display name of the entity property, e.g. "Relation_Currency". The metadata
    /// provider automatically handles this by using the Field0Default key and passing the localization
    /// of name as a parameter.
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Property)]
    public class DefaultDisplayAttribute : Attribute
    {
        /// <summary>
        /// The display name of the original entity property.
        /// </summary>
        public string Name { get; set; }
    }
}
