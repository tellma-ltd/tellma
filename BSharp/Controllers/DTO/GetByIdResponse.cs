using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    public class GetByIdResponse<TDto> where TDto : DtoBase
    {
        public TDto Result { get; set; }

        public string CollectionName { get; set; }
        
        public Dictionary<string, IEnumerable<DtoBase>> RelatedEntities { get; set; }
    }
}
