using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    public class DeactivateArguments<TKey>
    {
        [Required]
        public List<TKey> Ids { get; set; }
    }
}
