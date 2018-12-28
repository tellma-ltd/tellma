using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    public class DeactivateArguments<TKey>
    {
        /// <summary>
        /// Specifies that affected entities should be returned
        /// </summary>
        public bool? ReturnEntities { get; set; } = true;

        /// <summary>
        /// Specifies what navigation properties to expand in the returned entities
        /// (if <see cref="ActivateArguments{TKey}.ReturnEntities"/> is set to false
        /// this parameter will be ignored
        /// </summary>
        public string Expand { get; set; }
    }
}
