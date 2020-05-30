using Tellma.Controllers.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Services.GlobalSettings
{
    public interface IGlobalSettingsCache
    {
        Versioned<GlobalSettingsForClient> GetGlobalSettings();

        bool IsFresh(string version);

        void InvalidateCache();

    }
}
