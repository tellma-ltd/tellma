using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.OData
{
    public class SqlStatementParameters : IEnumerable<SqlParameter>
    {
        private readonly List<SqlParameter> _params = new List<SqlParameter>();

        public void AddParameter(SqlParameter p)
        {
            if(p.ParameterName.StartsWith("Param_"))
            {
                throw new InvalidOperationException("Cannot use reserved prefix 'Param_' in SQL parameter names");
            }

            _params.Add(p);
        }

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

        public IEnumerator<SqlParameter> GetEnumerator()
        {
            return _params.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _params.GetEnumerator();
        }
    }
}
