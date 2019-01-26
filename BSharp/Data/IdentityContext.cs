using BSharp.Data.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// dotnet ef migrations add <MigrationName> -c=IdentityContext -o=Data/Migrations/Identity
namespace BSharp.Data
{
    /// <summary>
    /// This is the identity context for 
    /// </summary>
    public class IdentityContext : IdentityUserContext<User>
    {
        public IdentityContext(DbContextOptions<AdminContext> opt) : base(opt) { }
    }
}
