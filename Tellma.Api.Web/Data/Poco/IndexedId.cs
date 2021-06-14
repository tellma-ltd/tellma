using Tellma.Model.Application;

namespace Tellma.Data
{
    /// <summary>
    /// Maps an entity Id to an index
    /// </summary>
    /// <typeparam name="TKey">The type of the Id</typeparam>
    public class IndexedId<TKey> : EntityWithKey<TKey>
    {
        public int Index { get; set; }
    }

    /// <summary>
    /// This one is commonly used to retrieve the newly created Ids of freshly saved entities
    /// </summary>
    public class IndexedId : IndexedId<int>
    {
    }
}
