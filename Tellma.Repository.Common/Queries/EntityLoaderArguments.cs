using System.Collections.Generic;
using Tellma.Repository.Common.Queryex;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// All the information required by the <see cref="IStatementLoader"/> to load a list of
    /// interrelated <see cref="SqlEntityStatement"/>s together with a total count.
    /// </summary>
    public class EntityLoaderArguments
    {
        /// <summary>
        /// The SQL code for loading the total unfiltered count of the root statement.
        /// <para/>
        /// If left null, a count of 0 is returned.
        /// </summary>
        public string CountSql { get; set; }

        /// <summary>
        /// List of interrelated <see cref="SqlEntityStatement"/> to load. 
        /// One of the statements must be the root statement.
        /// <para/>
        /// If the list is empty a null result is returned.
        /// </summary>
        public List<SqlEntityStatement> Statements { get; set; }

        /// <summary>
        /// Variable declarations needed to execute the statements.
        /// </summary>
        public SqlStatementVariables Variables { get; set; }

        /// <summary>
        /// Parameters needed to execute the statements.
        /// </summary>
        public SqlStatementParameters Parameters { get; set; }
    }
}
