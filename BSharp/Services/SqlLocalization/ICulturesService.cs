using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.SqlLocalization
{
    public interface ICulturesService
    {
        IEnumerable<string> GetActiveCultures();
    }
}
