using System;
using System.Collections.Generic;
using Tellma.Model.Common;

namespace Tellma.Api.Dto
{
    public class GetByIdResponse<TEntity> where TEntity : Entity
    {
        public Dictionary<string, object> Extras { get; set; }

        public TEntity Result { get; set; }

        public string CollectionName { get; set; }
        
        public Dictionary<string, IEnumerable<EntityWithKey>> RelatedEntities { get; set; }

        public DateTimeOffset ServerTime { get; set; }
    }
}
