using System.Collections.Generic;
using Tellma.Model.Common;

namespace Tellma.Controllers.Dto
{
    public class GetEntityResponse<TEntity>
    {
        public TEntity Result { get; set; }

        public Dictionary<string, IEnumerable<Entity>> RelatedEntities { get; set; }
    }
}
