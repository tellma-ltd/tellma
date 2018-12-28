using System.Collections.Generic;

namespace BSharp.Controllers.DTO
{
    /// <summary>
    /// Represents a response of raw entities for save
    /// </summary>
    public class EntitiesResponse<TDto> where TDto : DtoForSaveBase
    {
        public Dictionary<string, object> Bag { get; set; }

        public IEnumerable<TDto> Data { get; set; }

        public string CollectionName { get; set; }

        public Dictionary<string, IEnumerable<DtoForSaveBase>> RelatedEntities { get; set; }
    }

    /// <summary>
    /// Represents a response of raw entities for save + some search metadata
    /// </summary>
    public class GetResponse<TDto> : EntitiesResponse<TDto> where TDto : DtoForSaveBase
    {
        public int Skip { get; set; }

        public int Top { get; set; }

        public string OrderBy { get; set; }

        public bool Desc { get; set; }

        public int TotalCount { get; set; }
    }
}
