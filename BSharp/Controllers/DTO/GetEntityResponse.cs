using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    public class GetEntityResponse<TDto>
    {
        public TDto Result { get; set; }

        public Dictionary<string, IEnumerable<DtoBase>> Entities { get; set; }
    }
}
