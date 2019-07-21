using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    /// <summary>
    /// Represents a response of raw entities for save
    /// </summary>
    public class EntitiesResponse<TDto> where TDto : DtoBase
    {
        public Dictionary<string, object> Bag { get; set; }

        public IEnumerable<TDto> Result { get; set; }

        public string CollectionName { get; set; }

        public Dictionary<string, IEnumerable<DtoBase>> RelatedEntities { get; set; }

        public bool IsPartial { get; set; }
    }
}
