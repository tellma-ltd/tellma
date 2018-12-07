using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data.Model
{
    /// <summary>
    /// Represents a tenant-specific translation
    /// </summary>
    public class Translation : ModelForSaveBase, IAuditedModel
    {
        [Required]
        [MaxLength(50)]
        public string Tier { get; set; } // Client, Server, Shared

        [Required]
        [MaxLength(50)]
        public string Culture { get; set; } // ar-SA, en-GB, en, uz-Cyrl-UZ

        [Required]
        [MaxLength(450)]
        public string Name { get; set; } // The resource key

        [Required]
        [MaxLength(2048)]
        public string Value { get; set; } // The resource value

        // IAuditedModel
        public DateTimeOffset CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public DateTimeOffset ModifiedAt { get; set; }

        public string ModifiedBy { get; set; }
    }
}
