using Tellma.Entities;
using System.Collections.Generic;

namespace Tellma.Controllers.Dto
{
    public class GetByIdResponse<TEntity> where TEntity : Entity
    {
        public TEntity Result { get; set; }

        public string CollectionName { get; set; }
        
        public Dictionary<string, IEnumerable<Entity>> RelatedEntities { get; set; }
    }
}
