using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Shared
{
    public class GetByIdResponse<TDto> where TDto : DtoForSaveBase
    {
        public TDto Entity { get; set; }

        public Dictionary<string, IEnumerable<DtoForSaveBase>> RelatedEntities { get; set; }
    }
}
