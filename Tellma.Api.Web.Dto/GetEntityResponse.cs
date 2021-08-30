using System.Collections.Generic;
using Tellma.Model.Common;

namespace Tellma.Api.Dto
{
    public class GetEntityResponse<TEntity>
    {
        public TEntity Result { get; set; }

        public Dictionary<string, IEnumerable<EntityWithKey>> RelatedEntities { get; set; }
    }
}
