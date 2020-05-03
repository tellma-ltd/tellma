using Tellma.Entities;
using System.Collections.Generic;

namespace Tellma.Controllers.Dto
{
    public class GetEntityResponse<TEntity>
    {
        public TEntity Result { get; set; }

        public Dictionary<string, IEnumerable<Entity>> RelatedEntities { get; set; }
    }
}
