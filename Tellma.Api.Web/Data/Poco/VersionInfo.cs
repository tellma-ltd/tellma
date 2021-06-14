using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Data
{
    public class VersionInfo
    {
        /// <summary>
        /// Applies to the tenant as a whole
        /// </summary>
        public string SettingsVersion { get; set; }

        /// <summary>
        /// Applies to the tenant as a whole
        /// </summary>
        public string DefinitionsVersion { get; set; }

        /// <summary>
        /// Applies to the authenticated user
        /// </summary>
        public string PermissionsVersion { get; set; }

        /// <summary>
        /// Applies to the authenticated user
        /// </summary>
        public string UserSettingsVersion { get; set; }
    }
}
