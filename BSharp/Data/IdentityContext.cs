using BSharp.Data.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

// dotnet ef migrations add Initial -c=IdentityContext -o=Data/Migrations/Identity
namespace BSharp.Data
{
    /// <summary>
    /// This is the identity context for 
    /// </summary>
    public class IdentityContext : IdentityDbContext<User>
    {
        public IdentityContext(DbContextOptions<IdentityContext> opt) : base(opt) { }
    }
}
