using System;
using System.Globalization;
using System.Linq;

namespace BSharp.Entities
{
    /// <summary>
    /// Indicates that the adorned property can only be one of the supported system languages
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Property)]
    public class CultureAttribute : ChoiceListAttribute
    {
        public CultureAttribute() : base(Startup.SUPPORTED_CULTURES, GetCultureDisplayNames()) { }

        private static string[] GetCultureDisplayNames()
        {
            return Startup.SUPPORTED_CULTURES.Select(c => CultureInfo.GetCultureInfo(c)?.NativeName).ToArray();
        }
    }
}
