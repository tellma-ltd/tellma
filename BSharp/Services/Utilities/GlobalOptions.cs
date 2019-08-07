using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.Utilities
{
    public class GlobalOptions
    {
        public bool Online { get; set; } = true;
        public bool EmbeddedIdentityServerEnabled { get; set; } = true;
        public bool EmbeddedClientApplicationEnabled { get; set; } = true;

        public LocalizationOptions Localization { get; set; }

        public WebClientOptions ClientApplications { get; set; }
    }
}
