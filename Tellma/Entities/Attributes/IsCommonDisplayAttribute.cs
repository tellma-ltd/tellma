using System;

namespace Tellma.Entities
{
    /// <summary>
    /// Adorns is common properties (many are found in Document.cs), the Name in this case would be that
    /// of the property described by the is common property, e.g. CurrencyIsCommon would have name "Entry_Currency". 
    /// The metadata provider automatically handles this by using the Field0IsCommon key and passing the localization
    /// of name as a parameter
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Property)]
    public class IsCommonDisplayAttribute : Attribute
    {
        /// <summary>
        /// The display name of the original entity property
        /// </summary>
        public string Name { get; set; }
    }
}
