using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Shared
{
    public class GetByIdArguments
    {
        /// <summary>
        /// Equivalent to linq's "Include", determines which related entities to include in 
        /// the result, if left empty it means retrieve all properties
        /// </summary>
        public string Expand { get; set; }
    }
}
