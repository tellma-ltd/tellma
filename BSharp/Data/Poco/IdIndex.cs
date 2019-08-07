namespace BSharp.Data
{
    /// <summary>
    /// Maps an entity Id to an index
    /// </summary>
    /// <typeparam name="TKey">The type of the Id</typeparam>
    public class IdIndex<TKey>
    {
        public TKey Id { get; set; }

        public int Index { get; set; }
    }
}
