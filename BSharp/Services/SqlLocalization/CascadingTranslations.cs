using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.SqlLocalization
{
    public class CascadingTranslations
    {
        public string CultureName { get; set; }

        public Dictionary<string, string> TenantSpecificTranslations { get; set; }
        public Dictionary<string, string> CoreSpecificTranslations { get; set; }
        public Dictionary<string, string> TenantNeutralTranslations { get; set; }
        public Dictionary<string, string> CoreNeutralTranslations { get; set; }
        public Dictionary<string, string> TenantDefaultTranslations { get; set; }
        public Dictionary<string, string> CoreDefaultTranslations { get; set; }

        /// <summary>
        /// Returns the translation dictionaries in descending order of precendence
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Dictionary<string, string>> InDescendingOrderOfPrecedence()
        {
            yield return TenantSpecificTranslations; // highest precedence
            yield return CoreSpecificTranslations;
            yield return TenantNeutralTranslations;
            yield return CoreNeutralTranslations;
            yield return TenantDefaultTranslations;
            yield return CoreDefaultTranslations; // lowest precedence
        }
    }

}
