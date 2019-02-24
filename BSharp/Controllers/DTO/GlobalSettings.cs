using BSharp.Controllers.Misc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    public class GlobalSettingsForSave : DtoBase
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
        public Dictionary<string, Culture> ActiveCultures { get; set; }
    }
}
