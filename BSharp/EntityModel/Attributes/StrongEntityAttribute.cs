using System;

namespace BSharp.EntityModel
{
    /// <summary>
    /// This represents a strong Entity which has an API associated with and is returned and tracked in its own independent collection
    /// in API GET responses, as opposed to being a weak entity that is comes attached to another strong entity
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
