using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BSharp.Services.SqlLocalization
{
    /// <summary>
    /// Implementation of IStringLocalizer that relies on localizations retrieved
    /// from a SQL Server table
    /// </summary>
    public class SqlStringLocalizer : IStringLocalizer
    {
        private readonly SqlStringLocalizerFactory _factory;
        private CultureInfo _culture;

        public SqlStringLocalizer(SqlStringLocalizerFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public SqlStringLocalizer(SqlStringLocalizerFactory factory, CultureInfo culture) : this(factory)
        {
            _culture = culture;
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
            return new SqlStringLocalizer(_factory, culture);
        }

        /// <summary>
        /// Helper method
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string Localize(string name)
        {
            // The culture is either the fixed value or the cultureInfo of the current thread
            var culture = _culture ?? CultureInfo.CurrentUICulture;

            // We have no option here to use async/await since we are stuck with the non-asynchronous interface IStringLocalizer
            // However this is OK since the vast majority of requests are expected to be satisifed from the cache and won't be blocking
            return _factory.Localize(name, culture);
        }
    }
}
