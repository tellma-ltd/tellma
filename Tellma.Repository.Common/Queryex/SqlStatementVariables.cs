using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Tellma.Repository.Common.Queryex
{
    /// <summary>
    /// A collection containing a bunch of SQL variables. Contains a
    /// method that lets you add variables with auto-names: "Var__1", "Var__2" etc...
    /// </summary>
    public class SqlStatementVariables : IEnumerable<SqlVariable>
    {
        public const string _prefix = "Var__";
        private readonly HashSet<SqlVariable> _variables = new HashSet<SqlVariable>();
        private readonly Dictionary<string, SqlVariable> _definitionsDic = new Dictionary<string, SqlVariable>(StringComparer.OrdinalIgnoreCase); // Maps sqlDef to name
        private int _counter = 0;


        /// <summary>
        /// Add a statement variable with a specific name, (it's the job of the caller to ensure that variable names are unique).
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="sqlDef">The SQL definition (the part after DECLARE @Var.</param>
        public void AddVariable(SqlVariable variable)
        {
            if (variable.Name.StartsWith(_prefix))
            {
                // Developer mistake
                throw new InvalidOperationException($"Cannot use reserved prefix '{_prefix}' in SQL variable names");
            }

            if (variable.Name.StartsWith(SqlStatementParameters._prefix))
            {
                // Developer mistake
                throw new InvalidOperationException($"Cannot use reserved prefix '{SqlStatementParameters._prefix}' in SQL variable names");
            }

            if(_definitionsDic.TryAdd(variable.Definition, variable))
            {
                _variables.Add(variable);
            }
        }

        /// <summary>
        /// Add a statement variable with an automatically generated unique name.
        /// Variables with similar definitions are reused, comparison is case insensitive.
        /// </summary>
        /// <returns>The automatically generated unique name of the new statement variable.</returns>
        public string AddVariable(string type, string def)
        {
            if (!_definitionsDic.TryGetValue(def, out SqlVariable variable))
            {
                string name = $"{_prefix}{++_counter}";
                variable = new SqlVariable(name, type, def);

                _definitionsDic.Add(def, variable);
                _variables.Add(variable);
            }

            return variable.Name;
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable"/>.
        /// </summary>
        public IEnumerator<SqlVariable> GetEnumerator()
        {
            return _variables.GetEnumerator();
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable"/>.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _variables.GetEnumerator();
        }

        /// <summary>
        /// Returns the SQL declarations of the variables.
        /// </summary>
        public string ToSql()
        {
            var bldr = new StringBuilder();
            foreach (var variable in this)
            {
                bldr.AppendLine(variable.ToSql());
            }

            return bldr.ToString();
        }
    }

    public struct SqlVariable
    {
        public SqlVariable(string name, string type, string def)
        {
            Name = name;
            Type = type;
            Definition = def;
        }

        public string Name { get; }
        public string Type { get; }
        public string Definition { get; }

        public string ToSql()
        {
            return $"DECLARE @{Name} {Type} = {Definition};";
        }
    }
}
