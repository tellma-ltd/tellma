using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Represents an orderby argument which is a comma separated list of column accesses. 
    /// Some expressions may optionally be postfixed with "desc" or "asc" keywords. 
    /// For example: "Line.PostingDate desc,Id"
    /// </summary>
    public class ExpressionOrderBy : IEnumerable<QueryexColumnAccess>
    {
        private readonly IEnumerable<QueryexColumnAccess> _atoms;

        /// <summary>
        /// Create an instance of <see cref="ExpressionOrderBy"/> containing the provided <see cref="OrderByAtom"/>s
        /// </summary>
        public ExpressionOrderBy(IEnumerable<QueryexColumnAccess> atoms)
        {
            _atoms = atoms ?? throw new ArgumentNullException(nameof(atoms));
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable"/>
        /// </summary>
        public IEnumerator<QueryexColumnAccess> GetEnumerator()
        {
            return _atoms.GetEnumerator();
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
            if (!expressions.All(e => e is QueryexColumnAccess))
            {
                throw new QueryException($"OrderBy parameter can only contain column access expressions like this: Id desc,Name asc,CreatedBy.Name");
            }

            return new ExpressionOrderBy(expressions.Cast<QueryexColumnAccess>());
        }
    }
}
