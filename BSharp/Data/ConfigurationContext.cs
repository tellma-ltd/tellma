using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// dotnet ef migrations add Initial -c=ConfigurationContext -o=Data/Migrations/Configuration
namespace BSharp.Data
{
    public class ConfigurationContext : DbContext
    {
        public ConfigurationContext(DbContextOptions<ConfigurationContext> opt) : base(opt) { }

        // TODO
    }
}
