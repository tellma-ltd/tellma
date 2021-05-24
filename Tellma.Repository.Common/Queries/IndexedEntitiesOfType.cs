using System.Collections.Generic;
using Tellma.Model.Common;

namespace Tellma.Repository.Common
{    
    /// <summary>
     /// An instance of this class is used inside <see cref="StatementLoader"/> to track entities while hydrating them from the database.
     /// It maps every Id to its corresponding Entity, for efficient retrieval and to ensure that one C# object is created per Type and Id
     /// </summary>
    internal class IndexedEntitiesOfType : Dictionary<object, EntityWithKey> { }
}
