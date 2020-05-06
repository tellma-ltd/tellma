using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Represents a base <see cref="Templating.QueryInfo"/> and a select atom; a list of 0 or more steps from the root entity of the query.
    /// For example base query QueryById('documents/manual-journal-vouchers', 44) and steps ['CreatedBy', 'Name'].
    /// This data structure helps the algorithm that statically evaluates the SELECT list of the database queries
    /// that are needed to complete <see cref="TemplateBase.GenerateOutput(System.Text.StringBuilder, EvaluationContext)"/>;
    /// </summary>
    public class Path : IEnumerable<string>
    {
        private readonly IEnumerable<string> _steps;

        public QueryInfo QueryInfo { get; }

        public Path(QueryInfo baseQuery, IEnumerable<string> steps)
        {
            QueryInfo = baseQuery ?? throw new ArgumentNullException(nameof(baseQuery));
            _steps = steps ?? new List<string>();
        }

        /// <summary>
        /// Returns a new <see cref="Path"/> that is a clone of the old one with the supplied step appended to it
        /// </summary>
        public Path Append(string step) => new Path(QueryInfo, new List<string>(_steps) { step });

        /// <summary>
        /// Returns a new <see cref="Path"/> that is a clone of the old one with the supplied steps appended to it
        /// </summary>
        public Path Append(IEnumerable<string> steps) => new Path(QueryInfo, new List<string>(_steps.Concat(steps)));

        /// <summary>
        /// Returns a new empty <see cref="Path"/> (containing 0 steps) from the given query
        /// </summary>
        public static Path Empty(QueryInfo baseQuery) => new Path(baseQuery, new List<string>());

        #region IEnumerable Impl

        public IEnumerator<string> GetEnumerator()
        {
            return _steps.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _steps.GetEnumerator();
        }

        #endregion
    }
}
