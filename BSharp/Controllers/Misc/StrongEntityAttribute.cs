using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Misc
{
    /// <summary>
    /// This represents a strong dto which is returned and stored in its own centralized collection
    /// in API responses, as opposed to being a weak entity that is attached to another strong entity
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Class, Inherited = true)]
    public class StrongEntityAttribute : Attribute
    {
        public StrongEntityAttribute(Type type = null, bool isFact = false)
        {
            Type = type;
            IsFact = isFact;
        }

        public Type Type { get; private set; }
        public bool IsFact { get; private set; }
    }
}
