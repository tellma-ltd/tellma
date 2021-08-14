using System.Collections.Generic;

namespace Tellma.Api.Dto
{
    public class GetByIdsArguments<TKey> : SelectExpandArguments
    {
        /// <summary>
        /// The list of Ids to select.
        /// </summary>
        public List<TKey> I { get; set; }
    }
}
