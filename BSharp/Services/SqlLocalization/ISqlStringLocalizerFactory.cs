using BSharp.Controllers.DTO;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.SqlLocalization
{
    public interface ISqlStringLocalizerFactory : IStringLocalizerFactory
    {
        /// <summary>
        /// Forces a refresh from the database next time a localized resource is requested
        /// </summary>
        void InvalidateCache(string cultureName);

        /// <summary>
        /// Returns all the locaizations pertaining to the list of tiers provided
        /// </summary>
        DataWithVersion<Dictionary<string, string>> GetTranslations(string cultureName, params string[] tiers);

        /// <summary>
        /// Checks if the specified culture version is indeed the latest one
        /// </summary>
        bool IsFresh(string cultureName, string version);
    }
}
