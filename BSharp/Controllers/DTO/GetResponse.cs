using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    public class GetResponse<TDto> where TDto : DtoForSaveBase
    {
        public int Skip { get; set; }

        public int Top { get; set; }

        public string OrderBy { get; set; }

        public bool Desc { get; set; }

        public int TotalCount { get; set; }

        public Dictionary<string, object> Bag { get; set; }

        public Dictionary<string, IEnumerable<DtoForSaveBase>> RelatedEntities { get; set; }

        public IEnumerable<TDto> Data { get; set; }
    }
}
