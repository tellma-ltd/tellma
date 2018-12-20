using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.SqlLocalization
{
    public interface ICachingStringLocalizerFactory : IStringLocalizerFactory
    {
        Task InvalidateCacheAsync(string cultureName);
    }
}
