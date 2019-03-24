using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data.DbModel
{
    /// <summary>
    /// A Query type https://docs.microsoft.com/en-us/ef/core/modeling/query-types
    /// Used when importing from files, to efficiently query the database for 
    /// the Ids that correspond to a collection of codes
    /// </summary>
    public class CodeId
    {
        public int Id { get; set; }

        public string Code { get; set; }
    }
}
