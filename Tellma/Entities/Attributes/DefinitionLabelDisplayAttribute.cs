using System;

namespace Tellma.Entities
{
    [AttributeUsage(validOn: AttributeTargets.Property)]
    public class DefinitionLabelDisplayAttribute : Attribute
    {
        public string Name { get; set; }
        public Language Language { get; set; } = Language.Primary;
    }
}
