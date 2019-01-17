using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data.Model
{
    public class Agent : Custody
    {
        [Required]
        [MaxLength(255)]
        public string AgentType { get; set; }

        public bool IsRelated { get; set; }

        [MaxLength(255)]
        public string TaxIdentificationNumber { get; set; }

        [MaxLength(255)]
        public string Title { get; set; }

        [MaxLength(255)]
        public string Title2 { get; set; }

        public char? Gender { get; set; }

        internal static void OnModelCreating_Agent(ModelBuilder builder)
        {
            // IsRelated defaults to FALSE
            builder.Entity<Agent>().Property(e => e.IsRelated).HasDefaultValue(false);
        }
    }
}
