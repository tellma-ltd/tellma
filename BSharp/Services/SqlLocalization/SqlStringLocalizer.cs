using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace BSharp.Services.SqlLocalization
{
    /// <summary>
    /// Implementation of IStringLocalizer that relies on localizations retrieved
    /// from a SQL Server table
    /// </summary>
    public class SqlStringLocalizer : IStringLocalizer
    {
        private readonly SqlStringLocalizerFactory _factory;
        private CascadingTranslations _translations;
        private ReaderWriterLockSlim _translationsLock = new ReaderWriterLockSlim();

        public SqlStringLocalizer(SqlStringLocalizerFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// IStringLocalizer implementation
        /// </summary>
        public LocalizedString this[string name]
        {
            get
            {
                var value = Localize(name);

                // Return the value, or return the original name if the value was not found
                return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }
        }

        /// <summary>
        /// IStringLocalizer implementation
        /// </summary>
        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var format = Localize(name);
                var value = format == null ? null : string.Format(format, arguments);

                // Return the value, or return the original name if the value was not found
                return new LocalizedString(name, value ?? name, resourceNotFound: format == null);
            }
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Helper method
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string Localize(string name)
        {
            // Just in case 2 threads with 2 different cultures invoked the same localizer concurrently
            // (It probably never happens, but I have seen signs that some MVC libraries might do that)
            lock (_translationsLock)
            {
                // This is to workaround the behavior of certain MVC libraries
                // which seem to re-use the same IStringLocalizer across multiple requests,
                // even though the current UI culture is a scoped value and the localizer is transient
                if (_translations == null || CultureInfo.CurrentUICulture.Name != _translations.CultureName)
                {
                    // Check again inside the writer lock
                    if (_translations == null || CultureInfo.CurrentUICulture.Name != _translations.CultureName)
                    {
                        _translations = _factory.GetTranslationsForCurrentCulture();
                    }
                }

                // Go over the dictionaries one by one and return the first hit
                foreach (var translationDictionary in _translations.InDescendingOrderOfPrecedence())
                {
                    if (translationDictionary != null && translationDictionary.ContainsKey(name))
                    {
                        return translationDictionary[name];
                    }
                }

                // If all is a miss, return the name as is, forgiveness here is a virtue.
                return name;
            }
        }
    }
}
