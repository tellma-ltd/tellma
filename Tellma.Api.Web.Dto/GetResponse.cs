using Tellma.Model.Common;

namespace Tellma.Api.Dto
{
    /// <summary>
    /// Represents a response of raw entities for save + some search metadata.
    /// </summary>
    public class GetResponse<TEntity> : EntitiesResponse<TEntity> where TEntity : Entity
    {
        public int Skip { get; set; }

        public int Top { get; set; }

        public string OrderBy { get; set; }

        public int? TotalCount { get; set; }
    }
}
