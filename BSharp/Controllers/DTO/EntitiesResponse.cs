using BSharp.EntityModel;
using System.Collections.Generic;

namespace BSharp.Controllers.Dto
{
    /// <summary>
    /// Represents a response of raw entities for save
    /// </summary>
    public class EntitiesResponse<TEntity> where TEntity : Entity
    {
        public Dictionary<string, object> Bag { get; set; }

        public IEnumerable<TEntity> Result { get; set; }

        public string CollectionName { get; set; }

        public Dictionary<string, IEnumerable<Entity>> RelatedEntities { get; set; }

        public bool IsPartial { get; set; }
    }
}
