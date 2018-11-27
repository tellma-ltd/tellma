using BSharp.Data.Model.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// dotnet ef migrations add Initial -c=IdentityContext -o=Data/Migrations/Identity
namespace BSharp.Data
{
    /// <summary>
    /// The identity context of the application's embedded identity framework,
    /// it derived from IdentityUserContext to avoid adding unnecessary Roles table
    /// </summary>
    public class IdentityContext : IdentityUserContext<ApplicationUser>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options) { }
    }
}
