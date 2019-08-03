using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Data.Model
{
    public class IfrsConcept : ModelBase, IAuditedModel
    {
        [Required]
        [MaxLength(255)]
        public string Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string IfrsType { get; set; }

        [Required]
        [MaxLength(1024)]
        public string Label { get; set; }

        [MaxLength(1024)]
        public string Label2 { get; set; }

        [MaxLength(1024)]
        public string Label3 { get; set; }

        [Required]
        public string Documentation { get; set; }

        public string Documentation2 { get; set; }

        public string Documentation3 { get; set; }

        public DateTime EffectiveDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public int CreatedById { get; set; }
        public LocalUser CreatedBy { get; set; }

        public DateTimeOffset ModifiedAt { get; set; }

        public int ModifiedById { get; set; }
        public LocalUser ModifiedBy { get; set; }

        public IfrsNote Note { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            // Define the one to one relationships
            builder.Entity<IfrsConcept>()
                .HasOne(p => p.Note)
                .WithOne(i => i.Concept)
                .HasForeignKey<IfrsNote>(TenantId, nameof(Id));

            // IsActive defaults to TRUE
            builder.Entity<IfrsConcept>().Property(e => e.IsActive).HasDefaultValue(true);

            // IFRS Type defaults to Regulatory
            builder.Entity<IfrsConcept>().Property(e => e.IfrsType).HasDefaultValue("Regulatory");

            // Effective date defaults to the dawn of time
            builder.Entity<IfrsConcept>().Property(e => e.EffectiveDate).HasDefaultValueSql("'0001-01-01 00:00:00'");

            // Expiry date defaults to the end of time
            builder.Entity<IfrsConcept>().Property(e => e.ExpiryDate).HasDefaultValueSql("'9999-12-31 23:59:59'");

            // Tenant Id and Audit defaults
            builder.Entity<IfrsConcept>().Property(TenantId).HasDefaultValueSql("CONVERT(INT, SESSION_CONTEXT(N'TenantId'))");
            builder.Entity<IfrsConcept>().Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");
            builder.Entity<IfrsConcept>().Property(e => e.CreatedById).HasDefaultValueSql("CONVERT(INT, SESSION_CONTEXT(N'UserId'))");
            builder.Entity<IfrsConcept>().Property(e => e.ModifiedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");
            builder.Entity<IfrsConcept>().Property(e => e.ModifiedById).HasDefaultValueSql("CONVERT(INT, SESSION_CONTEXT(N'UserId'))");

            // Audit foreign keys
            builder.Entity<IfrsConcept>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(TenantId, nameof(CreatedById))
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<IfrsConcept>()
                .HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(TenantId, nameof(ModifiedById))
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
