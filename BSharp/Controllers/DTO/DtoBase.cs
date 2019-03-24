using BSharp.Controllers.Misc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    /// <summary>
    /// Only for type-safety during development, i.e to prevent the silly 
    /// mistake of passing model entities as DTO entities
    /// </summary>
    public class DtoBase
    {
        /// <summary>
        /// Contains metadata about the entity for client side consumption
        /// </summary>
        [NotMapped]
        [IgnoreInMetadata]
        public DtoMetadata EntityMetadata { get; set; } = new DtoMetadata();
    }
}
