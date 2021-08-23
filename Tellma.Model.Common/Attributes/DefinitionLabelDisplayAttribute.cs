using System;

namespace Tellma.Model.Common
{
    [AttributeUsage(validOn: AttributeTargets.Property)]
    public class DefinitionLabelDisplayAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
