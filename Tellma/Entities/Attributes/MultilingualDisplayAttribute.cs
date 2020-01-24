using System;

namespace Tellma.Entities
{
    /// <summary>
    /// Some entity properties are multilingual, for example "Name" also has "Name2" and "Name3" next to it.
    /// The display name for such a property is the same for all 3, but with a postfix representing the tenant's
    /// primary, secondary, and ternary languages. For example: "Name (English)", "Name (Amharic)", "Name (Chinese)".
    /// If the tenant has one language, then no postfix is added
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Property)]
    public class MultilingualDisplayAttribute : Attribute
    {
        public string Name { get; set; }
        public Language Language { get; set; } = Language.Primary;
    }
}
