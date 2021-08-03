using System;
using System.Globalization;
using System.Linq;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    /// <summary>
    /// Indicates that the adorned property can only be one of the supported system languages.
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Property)]
    public class CultureChoiceListAttribute : ChoiceListAttribute
    {
        public CultureChoiceListAttribute() : base(Cultures.SupportedCultures, GetCultureDisplayNames()) { }

        private static string[] GetCultureDisplayNames()
        {
            return Cultures.SupportedCultures.Select(c => CultureInfo.GetCultureInfo(c)?.NativeName).ToArray();
        }
    }
}
