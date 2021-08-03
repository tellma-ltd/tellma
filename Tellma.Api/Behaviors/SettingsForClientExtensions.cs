using Tellma.Api.Dto;

namespace Tellma.Api.Behaviors
{
    public static class SettingsForClientExtensions
    {
        /// <summary>
        /// Returns <paramref name="s"/>, <paramref name="s2"/> or <paramref name="s3"/> depending on whether
        /// the current UI culture matches the primary, secondary or ternary company language respectively.
        /// </summary>
        public static string Localize(this SettingsForClient settings, string s, string s2, string s3)
        {
            var cultureName = System.Globalization.CultureInfo.CurrentUICulture.Name;

            var currentLangIndex = cultureName == settings.TernaryLanguageId ? 3 : cultureName == settings.SecondaryLanguageId ? 2 : 1;
            return currentLangIndex == 3 ? (s3 ?? s) : currentLangIndex == 2 ? (s2 ?? s) : s;
        }
    }
}
