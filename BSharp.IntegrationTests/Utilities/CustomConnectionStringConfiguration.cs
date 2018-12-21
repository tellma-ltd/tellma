using BSharp.Services.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.IntegrationTests.Utilities
{
    /// <summary>
    /// Wrapper around an IConfiguration, keeps all configuration 
    /// the same except for the manager database connection string
    /// </summary>
    public class CustomConnectionStringConfiguration : IConfiguration
    {
        private readonly IConfiguration _config;
        private readonly string _adminDbName;

        public CustomConnectionStringConfiguration(IConfiguration config, string adminDbName)
        {
            _config = config;
            _adminDbName = adminDbName;
        }

        public string this[string key]
        {
            get
            {
                if (key == Constants.AdminConnection)
                {
                    return $"Server = .; Database = {_adminDbName}; Trusted_Connection = true; MultipleActiveResultSets = true";
                }

                return _config[key];
            }
            set
            {
                _config[key] = value;
            }
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return _config.GetChildren();
        }

        public IChangeToken GetReloadToken()
        {
            return _config.GetReloadToken();
        }

        public IConfigurationSection GetSection(string key)
        {
            var section = _config.GetSection(key);

            if (key == "ConnectionStrings")
            {
                return new CustomConnectionStringConfigurationSection(section, _adminDbName);
            }

            return section;
        }
    }
}
