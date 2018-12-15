using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.IntegrationTests.Utilities
{
    /// <summary>
    /// Compliments <see cref="CustomConnectionStringConfiguration"/>
    /// </summary>
    public class CustomConnectionStringConfigurationSection : CustomConnectionStringConfiguration, IConfigurationSection
    {
        private readonly IConfigurationSection _config;

        public CustomConnectionStringConfigurationSection(IConfigurationSection config, string managerDbName) : base(config, managerDbName)
        {
            _config = config;
        }

        public string Key => _config.Key;

        public string Path => _config.Path;

        public string Value { get => _config.Value; set => _config.Value = value; }
    }
}
