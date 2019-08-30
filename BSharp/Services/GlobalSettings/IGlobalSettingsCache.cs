using BSharp.Controllers.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.GlobalSettings
{
    public interface IGlobalSettingsCache
    {
        DataWithVersion<GlobalSettingsForClient> GetGlobalSettings();

        bool IsFresh(string version);

        void InvalidateCache();

    }
}
