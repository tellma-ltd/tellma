using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

// dotnet ef migrations add Initial -c=IdentityContext -o=Data/Migrations/Identity
namespace Tellma.Services.EmbeddedIdentityServer
{
    /// <summary>
    /// This is the context used by 
    /// </summary>
    public class EmbeddedIdentityServerContext : IdentityUserContext<EmbeddedIdentityServerUser>, IDataProtectionKeyContext
    {
        public EmbeddedIdentityServerContext(DbContextOptions<EmbeddedIdentityServerContext> opt) : base(opt) { }
        
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    }
}
