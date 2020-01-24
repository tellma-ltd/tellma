using Tellma.Entities;
using System;
using System.Collections.Generic;

namespace Tellma.Controllers.Dto
{
    // TODO: Delete (Not the ForClient part)
    public class GlobalSettingsForSave : Entity
    {
    }

    public class GlobalSettings : GlobalSettingsForSave
    {
        /// <summary>
        /// Changes whenever the global client settings change
        /// </summary>
        public Guid SettingsVersion { get; set; }
    }

    public class GlobalSettingsForClient
    {
    }
}
