using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Services.OData
{
    public class OrderByExpression : IEnumerable<OrderByAtom>
    {
        private IEnumerable<OrderByAtom> _atoms;

        public OrderByExpression(IEnumerable<OrderByAtom> atoms)
        {
            _atoms = atoms ?? throw new ArgumentNullException(nameof(atoms));
        }

        public IEnumerator<OrderByAtom> GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }

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
