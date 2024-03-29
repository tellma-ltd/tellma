﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Tellma.Repository.Common.Queryex
{
    /// <summary>
    /// A collection containing a bunch of <see cref="SqlParameter"/>s, contains a
    /// method that lets you add parameters with auto-names: "Param__1", "Param__2" etc...
    /// </summary>
    public class SqlStatementParameters : IEnumerable<SqlParameter>
    {
        public const string _prefix = "Param__";
        private readonly HashSet<SqlParameter> _params = new HashSet<SqlParameter>();
        private int _counter = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public SqlStatementParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SqlStatementParameters(params IEnumerable<SqlParameter>[] parameterGroups)
        {
            foreach (var parameterGroup in parameterGroups)
            {
                if (parameterGroup != null)
                {
                    foreach (var parameter in parameterGroup)
                    {
                        AddParameter(parameter);
                    }
                }
            }
        }

        /// <summary>
        /// Add a regular <see cref="SqlParameter"/> if not already added, the parameter name cannot start with "Param_"
        /// </summary>
        public void AddParameter(SqlParameter p)
        {
            if (p.ParameterName.StartsWith(_prefix))
            {
                // Developer mistake
                throw new InvalidOperationException($"Cannot use reserved prefix '{_prefix}' in SQL parameter names");
            }

            if (p.ParameterName.StartsWith(SqlStatementVariables._prefix))
            {
                // Developer mistake
                throw new InvalidOperationException($"Cannot use reserved prefix '{SqlStatementVariables._prefix}' in SQL parameter names");
            }

            _params.Add(p);
        }

        /// <summary>
        /// Add a parameter with an automatically generated unique name
        /// </summary>
        /// <returns>The automatically generated unique name of the new <see cref="SqlParameter"/></returns>
        public string AddParameter(object value)
        {
            string paramName = $"{_prefix}{++_counter}";

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
