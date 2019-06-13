using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace BSharp.Data.Model
{
    public class ProductCategory : ModelBase, IAuditedModel
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }
        public ProductCategory Parent { get; set; }
        public ICollection<ProductCategory> Children { get; set; }

        public string Node { get; set; }

        public short Level { get; set; }

        public string ParentNode { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Name2 { get; set; }

        [MaxLength(255)]
        public string Name3 { get; set; }

        [MaxLength(255)]
        public string Code { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public int CreatedById { get; set; }
        public LocalUser CreatedBy { get; set; }

        public DateTimeOffset ModifiedAt { get; set; }

        public int ModifiedById { get; set; }
        public LocalUser ModifiedBy { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            // IsActive defaults to TRUE
            builder.Entity<ProductCategory>().Property(e => e.IsActive).HasDefaultValue(true);

            // Code is unique
            builder.Entity<ProductCategory>().HasIndex(TenantId, nameof(Code)).IsUnique();

            // Tenant Id and Audit defaults
            builder.Entity<ProductCategory>().Property(TenantId).HasDefaultValueSql("CONVERT(INT, SESSION_CONTEXT(N'TenantId'))");
            builder.Entity<ProductCategory>().Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");
            builder.Entity<ProductCategory>().Property(e => e.CreatedById).HasDefaultValueSql("CONVERT(INT, SESSION_CONTEXT(N'UserId'))");
            builder.Entity<ProductCategory>().Property(e => e.ModifiedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");
            builder.Entity<ProductCategory>().Property(e => e.ModifiedById).HasDefaultValueSql("CONVERT(INT, SESSION_CONTEXT(N'UserId'))");

            // Foreign Key
            builder.Entity<ProductCategory>()
                .HasMany(e => e.Children)
                .WithOne(e => e.Parent)
                .HasForeignKey(TenantId, nameof(ParentId)).OnDelete(DeleteBehavior.SetNull);

            // Audit foreign keys
            builder.Entity<ProductCategory>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(TenantId, nameof(CreatedById))
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductCategory>()
                .HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(TenantId, nameof(ModifiedById))
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
