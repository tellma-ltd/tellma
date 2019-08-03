using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data.Model
{
    /// <summary>
    /// MANAGER DB
    /// This is the table of users that resides in the manager DB, it contains all users of the application
    /// </summary>
    public class GlobalUser : ModelBase
    {
        public int Id { get; set; }

        [MaxLength(450)]
        public string ExternalId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Email { get; set; }

        public ICollection<TenantMembership> Memberships { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            // ExternalId is unique
            builder.Entity<GlobalUser>().HasIndex(nameof(ExternalId)).IsUnique();

            // Email is unique
            builder.Entity<GlobalUser>().HasIndex(nameof(Email)).IsUnique();
        }
    }
}
