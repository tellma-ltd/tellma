using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace BSharp.Data.Queries
{
    /// <summary>
    /// A collection containing a bunch of <see cref="SqlParameter"/>s, contains a
    /// method that lets you add parameters with auto-names: "Param_1", "Param_2" etc...
    /// </summary>
    public class SqlStatementParameters : IEnumerable<SqlParameter>
    {
        private readonly List<SqlParameter> _params = new List<SqlParameter>();

        /// <summary>
        /// Add a regular <see cref="SqlParameter"/>, the parameter name cannot start with "Param_"
        /// </summary>
        public void AddParameter(SqlParameter p)
        {
            if(p.ParameterName.StartsWith("Param_"))
            {
                throw new InvalidOperationException("Cannot use reserved prefix 'Param_' in SQL parameter names");
            }

            _params.Add(p);
        }

        /// <summary>
        /// Add a parameter with an automatically generated unique name
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The automatically generated unique name of the new <see cref="SqlParameter"/></returns>
        public string AddParameter(object value)
        {
            int nextId = _params.Count + 1;
            string paramName = $"Param_{nextId}";
            _params.Add(new SqlParameter
            {
                ParameterName = paramName,
                Value = value ?? DBNull.Value
            });

            return paramName;
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable"/>
        /// </summary>
        public IEnumerator<SqlParameter> GetEnumerator()
        {
            return _params.GetEnumerator();
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable"/>
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _params.GetEnumerator();
        }
    }
}
