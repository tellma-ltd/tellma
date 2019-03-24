using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Data.Model
{
    public class Custody : ModelBase, IAuditedModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string CustodyType { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Name2 { get; set; }

        [MaxLength(255)]
        public string Code { get; set; }

        [MaxLength(1024)]
        public string Address { get; set; }

        public DateTimeOffset? BirthDateTime { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public int CreatedById { get; set; }
        public LocalUser CreatedBy { get; set; }

        public DateTimeOffset ModifiedAt { get; set; }

        public int ModifiedById { get; set; }
        public LocalUser ModifiedBy { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            // Map the discriminator column to the concrete column
            builder.Entity<Custody>().HasDiscriminator<string>(nameof(CustodyType));

            // IsActive defaults to TRUE
            builder.Entity<Custody>().Property(e => e.IsActive).HasDefaultValue(true);

            // Code is unique
            builder.Entity<Custody>().HasIndex(TenantId, nameof(Code)).IsUnique();

            // Audit foreign keys
            builder.Entity<Custody>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(TenantId, nameof(CreatedById))
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Custody>()
                .HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(TenantId, nameof(ModifiedById))
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
