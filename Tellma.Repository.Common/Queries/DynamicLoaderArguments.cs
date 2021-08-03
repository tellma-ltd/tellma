using System.Collections.Generic;
using Tellma.Repository.Common.Queryex;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// All the information required by the <see cref="IStatementLoader"/> to load a single
    /// <see cref="SqlDynamicStatement"/>s together with a total count.
    /// </summary>
    public class DynamicLoaderArguments
    {
        /// <summary>
        /// The SQL code for loading the total unfiltered count of the principal statement.
        /// <para/>
        /// If left null, a count of 0 is returned.
        /// </summary>
        public string CountSql { get; set; }

        /// <summary>
        /// The principal statement to load.
        /// </summary>
        public SqlDynamicStatement PrincipalStatement { get; set; }

        /// <summary>
        /// Each statement in this list is a tree dimension that we wish to expand. 
        /// The statement will return the ancestors that are not returned by the principal query.
        /// </summary>
        public List<SqlDimensionAncestorsStatement> DimensionAncestorsStatements { get; set; }

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
