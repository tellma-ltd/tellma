using BSharp.EntityModel;
using System.Collections.Generic;

namespace BSharp.Controllers.Dto
{
    public class GetEntityResponse<TEntity>
    {
        public TEntity Result { get; set; }

        public Dictionary<string, IEnumerable<Entity>> Entities { get; set; }
    }
}
