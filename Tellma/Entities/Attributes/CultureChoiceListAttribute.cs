using System;
using System.Globalization;
using System.Linq;

namespace Tellma.Entities
{
    /// <summary>
    /// Indicates that the adorned property can only be one of the supported system languages
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Property)]
    public class CultureChoiceListAttribute : ChoiceListAttribute
    {
        public CultureChoiceListAttribute() : base(Startup.SUPPORTED_CULTURES, GetCultureDisplayNames()) { }

        private static string[] GetCultureDisplayNames()
        {
            return Startup.SUPPORTED_CULTURES.Select(c => CultureInfo.GetCultureInfo(c)?.NativeName).ToArray();
        }
    }
}
