using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Misc
{
    /// <summary>
    /// Specifies that a certain property is not to be included in the DTO metadata, this is used for base
    /// properties that are always incldued such as Id or the EntityMetadata proeprty itself
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreInMetadataAttribute : Attribute
    {
    }
}
