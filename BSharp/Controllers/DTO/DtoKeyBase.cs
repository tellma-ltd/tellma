using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    public class DtoKeyBase<TKey> : DtoBase
    {
        /// <summary>
        /// This is an integer for entities that have a simple integer key in the SQL database,
        /// and a string for anything else (The string can encode composite keys for example) 
        /// it is important to have a single Id property for tracking HTTP resources as it simplifies
        /// so much shared logic for tracking resources and caching them
        /// </summary>
        public TKey Id { get; set; }
    }
}
