using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Data.Model
{
    // Note: When modifying these properties, remember to modify the Application Sources SQL too
    public class IfrsNote : ModelBase
    {
        [Required]
        [MaxLength(255)]
        public string Id { get; set; }

        public string Node { get; set; }

        public short Level { get; set; }

        public string ParentNode { get; set; }

        public bool IsAggregate { get; set; }

        public bool ForDebit { get; set; }

        public bool ForCredit { get; set; }

        public IfrsConcept Concept { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            // IsAggregate defaults to TRUE
            builder.Entity<IfrsNote>().Property(e => e.IsAggregate).HasDefaultValue(true);

            // ForDebit defaults to TRUE
            builder.Entity<IfrsNote>().Property(e => e.ForDebit).HasDefaultValue(true);

            // ForCredit defaults to TRUE
            builder.Entity<IfrsNote>().Property(e => e.ForCredit).HasDefaultValue(true);

            // Tenant Id
            builder.Entity<IfrsNote>().Property(TenantId).HasDefaultValueSql("CONVERT(INT, SESSION_CONTEXT(N'TenantId'))");
        }
    }
}
