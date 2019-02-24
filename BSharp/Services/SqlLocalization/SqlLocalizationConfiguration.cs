using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.SqlLocalization
{
    public class SqlLocalizationConfiguration
    {
        public string DefaultCulture { get; set; } = "en-GB";
        public string DefaultUICulture { get; set; } = "en";
        public int CacheExpirationMinutes { get; set; } = 120;
    }
}
