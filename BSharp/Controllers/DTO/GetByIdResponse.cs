using BSharp.EntityModel;
using System.Collections.Generic;

namespace BSharp.Controllers.Dto
{
    public class GetByIdResponse<TEntity> where TEntity : Entity
    {
        public TEntity Result { get; set; }

        public string CollectionName { get; set; }
        
        public Dictionary<string, IEnumerable<Entity>> RelatedEntities { get; set; }
    }
}
