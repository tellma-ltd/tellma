using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

// dotnet ef migrations add Initial -c=IdentityContext -o=Data/Migrations/Identity
namespace BSharp.Services.EmbeddedIdentityServer
{
    /// <summary>
    /// This is the context used by 
    /// </summary>
    public class EmbeddedIdentityServerContext : IdentityDbContext<EmbeddedIdentityServerUser>
    {
        public EmbeddedIdentityServerContext(DbContextOptions<EmbeddedIdentityServerContext> opt) : base(opt) { }
    }
}
