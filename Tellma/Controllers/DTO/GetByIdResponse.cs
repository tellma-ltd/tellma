using Tellma.Entities;
using System.Collections.Generic;
using System;

namespace Tellma.Controllers.Dto
{
    public class GetByIdResponse<TEntity> where TEntity : Entity
    {
        public Dictionary<string, object> Extras { get; set; }

        public TEntity Result { get; set; }

        public string CollectionName { get; set; }
        
        public Dictionary<string, IEnumerable<Entity>> RelatedEntities { get; set; }

        public DateTimeOffset ServerTime { get; internal set; }
    }
}
