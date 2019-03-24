using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Misc
{
    /// <summary>
    /// Every navigation property on a DTO must be adorned with this attribute specifying the 
    /// foreign key that compliments the navigation property, or null if no such field exists
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NavigationPropertyAttribute : Attribute
    {
        /// <summary>
        /// The foreign key that compliments the navigation property
        /// </summary>
        public string ForeignKey { get; set; }
    }
}
