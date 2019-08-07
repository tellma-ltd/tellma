using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Data.Queries
{
    /// <summary>
    /// Represents an expand argument which is a comma separated list of paths. Where some paths are optionally
    /// postfixed with "desc" or "asc". For example: "Invoice/Customer,Amount"
    /// </summary>
    public class OrderByExpression : IEnumerable<OrderByAtom>
    {
        private readonly IEnumerable<OrderByAtom> _atoms;

        /// <summary>
        /// Create an instance of <see cref="OrderByExpression"/> containing the provided <see cref="OrderByAtom"/>s
        /// </summary>
        public OrderByExpression(IEnumerable<OrderByAtom> atoms)
        {
            _atoms = atoms ?? throw new ArgumentNullException(nameof(atoms));
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable"/>
        /// </summary>
        public IEnumerator<OrderByAtom> GetEnumerator()
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
        /// Parses a string representing an order by argument into an <see cref="OrderByExpression"/>. 
        /// The orderby argument is a comma separated list of paths, where some paths are optionally
        /// postfixed with "desc" or "asc". For example "Invoice/Customer,Amount"
        /// </summary>
        public static OrderByExpression Parse(string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return null;
            }

            var atoms = orderby
                .Split(',')
                .Select(e => e?.Trim())
                .Where(e => !string.IsNullOrEmpty(e))
                .Select(s => OrderByAtom.Parse(s));

            return new OrderByExpression(atoms);
        }
    }
}
