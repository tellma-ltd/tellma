using BSharp.Data;
using BSharp.Services.MultiTenancy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
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
        /// <summary>
        ///  This is used to clear the cache on demand
        /// </summary>
        private static CancellationTokenSource _source = new CancellationTokenSource();

        // below are the dictionaries 
        private readonly Dictionary<string, string> _coreSpecificTranslations;
        private readonly Dictionary<string, string> _coreNeutralTranslations;
        private readonly Dictionary<string, string> _coreDefaultTranslations;
        private readonly Dictionary<string, string> _tenantSpecificTranslations;
        private readonly Dictionary<string, string> _tenantNeutralTranslations;
        private readonly Dictionary<string, string> _tenantDefaultTranslations;

        public SqlStringLocalizer(
            Dictionary<string, string> coreSpecificTranslations,
            Dictionary<string, string> coreNeutralTranslations,
            Dictionary<string, string> coreDefaultTranslations)
        {
            _coreSpecificTranslations = coreSpecificTranslations ?? 
                throw new ArgumentNullException(nameof(coreSpecificTranslations));

            _coreNeutralTranslations = coreNeutralTranslations ?? 
                throw new ArgumentNullException(nameof(coreNeutralTranslations));

            _coreDefaultTranslations = coreDefaultTranslations ?? 
                throw new ArgumentNullException(nameof(coreDefaultTranslations));
        }

        public SqlStringLocalizer(
            Dictionary<string, string> coreSpecificTranslations,
            Dictionary<string, string> coreNeutralTranslations,
            Dictionary<string, string> coreDefaultTranslations,
            Dictionary<string, string> tenantSpecificTranslations,
            Dictionary<string, string> tenantNeutralTranslations,
            Dictionary<string, string> tenantDefaultTranslations) : 
            this(coreSpecificTranslations, coreNeutralTranslations, coreDefaultTranslations)
        {
            _tenantSpecificTranslations = tenantSpecificTranslations ?? 
                throw new ArgumentNullException(nameof(tenantSpecificTranslations));

            _tenantNeutralTranslations = tenantNeutralTranslations ?? 
                throw new ArgumentNullException(nameof(tenantNeutralTranslations));

            _tenantDefaultTranslations = tenantDefaultTranslations ?? 
                throw new ArgumentNullException(nameof(tenantDefaultTranslations));
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
            // Prepare the translation dictionaries in order of precedence
            Dictionary<string, string>[] translationDictionaries =  {
                _tenantSpecificTranslations, // highest precedence
                _coreSpecificTranslations,
                _tenantNeutralTranslations,
                _coreNeutralTranslations,
                _tenantDefaultTranslations,
                _coreDefaultTranslations // lowest precedence
            };
            
            // Go over them one by one and return the first hit
            for(int i=0; i< translationDictionaries.Length; i++)
            {
                var translationDictionary = translationDictionaries[i];
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
