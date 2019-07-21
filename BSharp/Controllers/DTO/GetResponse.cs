using System.Collections;
using System.Collections.Generic;

namespace BSharp.Controllers.DTO
{
    /// <summary>
    /// Represents a response of raw entities for save + some search metadata
    /// </summary>
    public class GetResponse<TDto> : EntitiesResponse<TDto> where TDto : DtoBase
    {
        public int Skip { get; set; }

        public int Top { get; set; }

        public string OrderBy { get; set; }

        public int TotalCount { get; set; }
    }
}
