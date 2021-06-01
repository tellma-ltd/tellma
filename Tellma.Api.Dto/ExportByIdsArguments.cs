using System.Collections.Generic;

namespace Tellma.Api.Dto
{
    public class ExportByIdsArguments<TKey>
    {
        /// <summary>
        /// The list of Ids to select.
        /// <para/>
        /// Note: the parameter name is kept short (i=1&i=2&i=3 etc...) because otherwise it will bloat the query string.
        /// </summary>
        public List<TKey> I { get; set; }
    }
}
