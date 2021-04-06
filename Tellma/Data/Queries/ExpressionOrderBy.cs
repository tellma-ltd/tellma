using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Represents an aggregate orderby argument which is a comma separated list of non-aggregated non-boolean expressions. 
    /// The syntax is anything that can be compiled by <see cref="QueryexBase"/>.
    /// Some expressions may optionally be postfixed with "desc" or "asc" keywords. 
    /// For example: "Line.PostingDate desc,Amount * Value"
    /// </summary>
    public class ExpressionOrderBy : IEnumerable<QueryexBase>
    {
        private readonly IEnumerable<QueryexBase> _atoms;

        /// <summary>
        /// Create an instance of <see cref="ExpressionOrderBy"/> containing the provided <see cref="OrderByAtom"/>s
        /// </summary>
        public ExpressionOrderBy(IEnumerable<QueryexBase> atoms)
        {
            _atoms = atoms ?? throw new ArgumentNullException(nameof(atoms));

            var aggregation = atoms.SelectMany(e => e.Aggregations()).FirstOrDefault();
            if (aggregation != null)
            {
                throw new QueryException($"OrderBy parameter cannot contain aggregation functions like {aggregation.Name}.");
            }
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable"/>
        /// </summary>
        public IEnumerator<QueryexBase> GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }

        public IEnumerable<QueryexColumnAccess> ColumnAccesses()
        {
            return _atoms.SelectMany(e => e.ColumnAccesses());
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable"/>
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }

        /// <summary>
        /// Parses a string representing an order by argument into an <see cref="ExpressionOrderBy"/>. 
        /// The orderby argument is a comma separated list of paths, where some paths are optionally
        /// postfixed with "desc" or "asc". 
        /// For example: "Line.PostingDate desc,Id"
        /// </summary>
        public static ExpressionOrderBy Parse(string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return null;
            }

            var expressions = QueryexBase.Parse(orderby, expectDirKeywords: true);
            return new ExpressionOrderBy(expressions);
        }
    }
}
