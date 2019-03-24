using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    public class GetByIdArguments
    {
        /// <summary>
        /// Equivalent to linq's "Include", determines which related entities to include in 
        /// the result, if left empty it means retrieve all properties
        /// </summary>
        public string Expand { get; set; }

        /// <summary>
        /// Equivalent to linq's "Select", determines which properties of the principle entities
        /// or of the included related entities to return the result. If left empty then all
        /// properties of the principle entity and included entities are returned
        /// </summary>
        public string Select { get; set; }
    }
}
