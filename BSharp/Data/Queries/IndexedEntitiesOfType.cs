using BSharp.EntityModel;
using System.Collections.Generic;

namespace BSharp.Data.Queries
{    
    /// <summary>
     /// An instance of this class is used inside <see cref="EntityLoader"/> to track entities while hydrating them from the database.
     /// It maps every Id to its corresponding Entity, for efficient retrieval and to ensure that one C# object is created per Type and Id
     /// </summary>
    public class IndexedEntitiesOfType : Dictionary<object, EntityWithKey> { }
}
