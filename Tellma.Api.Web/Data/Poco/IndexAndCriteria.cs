using Tellma.Model.Application;

namespace Tellma.Data
{
    /// <summary>
    /// Maps a criteria string to an Index
    /// </summary>
    public class IndexAndCriteria : Entity
    {
        public string Criteria { get; set; }

        public int Index { get; set; }
    }
}
