using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data.Model.Application
{
    /// <summary>
    /// A Query type https://docs.microsoft.com/en-us/ef/core/modeling/query-types
    /// represents the structure of validation errors that are reported by SQL stored procedures
    /// </summary>
    public class SqlValidationResult
    {
        public string Key { get; set; }

        public string ErrorName { get; set; }

        public string Argument1 { get; set; }

        public string Argument2 { get; set; }

        public string Argument3 { get; set; }

        public string Argument4 { get; set; }

        public string Argument5 { get; set; }
    }
}
