using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Misc
{
    /// <summary>
    /// This represents the type name of the entities from the point of view of the API consumers
    /// By default the type name is retrieved with Type.Name, but this is available for entities 
    /// that wish to override this behavior
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Class, Inherited = true)]
    public class StrongDtoAttribute : Attribute
    {
        public StrongDtoAttribute(string collectionName)
        {
            CollectionName = collectionName;
        }

        public string CollectionName { get; set; }
    }
}
