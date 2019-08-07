using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Misc
{
    // TODO: Delete

    /// <summary>
    /// Every property on a DTO representing a foreign key must be adorned with this attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : Attribute
    {
        // public string NavigationProperty { get; set; }
    }
}
