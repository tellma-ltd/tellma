using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    public class SaveGlobalSettingsResponse : GetByIdResponse<GlobalSettings>
    {
        public DataWithVersion<GlobalSettingsForClient> SettingsForClient { get; set; }
    }
}
