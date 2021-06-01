using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;

namespace Tellma.Api.Tests
{
    class NullStringLocalizer : IStringLocalizer<Strings>
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] => new(name, name);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            yield break;
        }
    }
}
