using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data.CrudModel
{
    public class CrudModelKeyBase<TKey>
    {
        /// <summary>
        /// This is an integer for entities that have a simple integer key in the SQL database,
        /// and a string for anything else (The string can encode composite keys for example) 
        /// it is important to have a single Id property for tracking HTTP resources as it simplifies
        /// so much shared logic for tracking resources and caching them
        /// </summary>
        public TKey Id { get; set; }

        [NotMapped]
        public EntityMetadata EntityMetadata { get; set; } = new EntityMetadata();
    }
}
