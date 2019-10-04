using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Dto
{
    public class GetChildrenArguments<TKey> : GetByIdArguments
    {
        /// <summary>
        /// The Ids of the parents whose children is to return
        /// </summary>
        public List<TKey> Ids { get; set; }

        /// <summary>
        /// Whether to return all nodes in the query or only active ones
        /// </summary>
        public string Filter { get; set; }
    }
}
