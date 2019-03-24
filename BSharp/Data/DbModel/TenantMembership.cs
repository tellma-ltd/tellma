using Microsoft.EntityFrameworkCore;

namespace BSharp.Data.DbModel
{
    /// <summary>
    /// MANAGER DB
    /// This class represents a memberhip of a global user in a certain tenant
    /// </summary>
    public class TenantMembership
    {
        public int UserId { get; set; }
        public GlobalUser User { get; set; }

        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            // Composite primary key
            builder.Entity<TenantMembership>().HasKey(e => new { e.UserId, e.TenantId });
        }
    }
}
