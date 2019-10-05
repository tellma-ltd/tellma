using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Dto
{
    public class GetChildrenArguments<TKey> : GetByIdArguments
    {
        /// <summary>
        /// The Ids of the parents whose children is to return, the property name is made as
        /// small as possible so that as many ids can fit in the query string as possible: like
        /// this api/bla?i=1&i=2&i=3&i=4&i=5&i=6&i=7&i=8&i=9
        /// </summary>
        public List<TKey> I { get; set; }

        /// <summary>
        /// Whether to return all nodes in the query or only active ones
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Determines whether the roots should be returned as well
        /// </summary>
        public bool Roots { get; set; }
    }
}
