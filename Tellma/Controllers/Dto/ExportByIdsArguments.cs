using System.Collections.Generic;

namespace Tellma.Controllers.Dto
{
    public class ExportByIdsArguments<TKey>
    {
        /// <summary>
        /// The list of Ids to select, the name is kept short because it will bloat the query string: i=1&i=2&i=3 etc...
        /// </summary>
        public List<TKey> I { get; set; }
    }
}
