using BSharp.Controllers.DTO;
using BSharp.Services.OData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.OData
{
    public class ObjectLoaderResult
    {
        public List<DtoBase> Result { get; set; }

        public IndexedEntities StrongIdEntities { get; set; }
    }
}
