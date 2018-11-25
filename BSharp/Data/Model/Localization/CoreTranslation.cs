using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data.Model.Localization
{
    /// <summary>
    /// Represents a core translation, shared across all tenants
    /// </summary>
    public class CoreTranslation
    {
        [Required]
        [MaxLength(50)]
        public string Tier { get; set; } // Client, C#, SQL, Other

        [Required]
        [MaxLength(50)]
        public string Culture { get; set; } // ar-SA, en-GB, en, uz-Cyrl-UZ

        [Required]
        [MaxLength(450)]
        public string Name { get; set; } // The resource key

        [Required]
        [MaxLength(2048)]
        public string Value { get; set; } // The resource value
    }
}
