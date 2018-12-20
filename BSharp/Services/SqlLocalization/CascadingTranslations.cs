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

        public Dictionary<string, string> SpecificTranslations { get; set; } // ar-SA
        public Dictionary<string, string> NeutralTranslations { get; set; } // ar
        public Dictionary<string, string> DefaultTranslations { get; set; } // en

        /// <summary>
        /// Returns the translation dictionaries in descending order of precendence
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Dictionary<string, string>> InDescendingOrderOfPrecedence()
        {
            yield return SpecificTranslations; // highest precedence
            yield return NeutralTranslations;
            yield return DefaultTranslations; // lowest precedence
        }
    }

}
