using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.SqlLocalization
{
    /// <summary>
    /// Stores all the translations that an instance of <see cref="SqlStringLocalizer"/> relies on
    /// it also provides a convenience method that orders the translations by precendece
    /// </summary>
    public class CascadingTranslations
    {
        public string CultureName { get; set; }

        public Dictionary<string, string> TenantSpecificTranslations { get; set; } // ar-SA, 101
        public Dictionary<string, string> CoreSpecificTranslations { get; set; } // ar-SA
        public Dictionary<string, string> TenantNeutralTranslations { get; set; } // ar, 101
        public Dictionary<string, string> CoreNeutralTranslations { get; set; } // ar
        public Dictionary<string, string> TenantDefaultTranslations { get; set; } // en, 101
        public Dictionary<string, string> CoreDefaultTranslations { get; set; } // en

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
