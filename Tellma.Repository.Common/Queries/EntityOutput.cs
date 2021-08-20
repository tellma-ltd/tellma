using System.Collections.Generic;
using Tellma.Model.Common;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Packages the result of loading an <see cref="SqlEntityStatement"/>.
    /// </summary>
    public class EntityOutput<TEntity> where TEntity : Entity
    {
        public EntityOutput(List<TEntity> entities, int count)
        {
            Entities = entities;
            Count = count;
        }

        /// <summary>
        /// The entities returned from the DB.
        /// </summary>
        public List<TEntity> Entities { get; set; }

        /// <summary>
        /// The total count of the unfiltered entities if such a count is requested.
        /// </summary>
        public int Count { get; set; }
    }
}
