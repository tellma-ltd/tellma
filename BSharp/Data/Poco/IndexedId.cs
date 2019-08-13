namespace BSharp.Data
{
    /// <summary>
    /// Maps an entity Id to an index
    /// </summary>
    /// <typeparam name="TKey">The type of the Id</typeparam>
    public class IndexedId<TKey>
    {
        public TKey Id { get; set; }

        public int Index { get; set; }
    }

    /// <summary>
    /// This one is commonly used to retrieve the newly created Ids of freshly saved entities
    /// </summary>
    public class IndexedId : IndexedId<int>
    {
    }
}
