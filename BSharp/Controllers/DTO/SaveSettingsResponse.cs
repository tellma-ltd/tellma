using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    public class SaveSettingsResponse : GetEntityResponse<Settings>
    {
        public DataWithVersion<SettingsForClient> SettingsForClient { get; set; }
    }
}
