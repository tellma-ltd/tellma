using System.Collections.Generic;
using Tellma.Api.Dto;

namespace Tellma.Controllers.Dto
{
    public class GetByIdsArguments<TKey> : SelectExpandArguments
    {
        /// <summary>
        /// The list of Ids to select
        /// </summary>
        public List<TKey> I { get; set; }
    }
}
