using System;

namespace Tellma.Entities
{
    /// <summary>
    /// Adorns definition properties that store the display of entity properties, e.g. CurrencyVisibility
    /// The name refers to the display name of the entity property, e.g. "Contract_Currency". The metadata
    /// provider automatically handles this by using the Field0Visibility key and passing the localization
    /// of name as a parameter
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Property)]
    public class VisibilityDisplayAttribute : Attribute
    {
        /// <summary>
        /// The display name of the original entity property
        /// </summary>
        public string Name { get; set; }
    }
}
