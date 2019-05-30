using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.OData
{
    public class SelectAggregationExpression : IEnumerable<SelectAggregationAtom>
    {
        private IEnumerable<SelectAggregationAtom> _atoms;

        public SelectAggregationExpression(IEnumerable<SelectAggregationAtom> atoms)
        {
            _atoms = atoms ?? throw new ArgumentNullException(nameof(atoms));
        }

        public IEnumerator<SelectAggregationAtom> GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }

        public static SelectAggregationExpression Parse(string select)
        {
            if (string.IsNullOrWhiteSpace(select))
            {
                throw new ArgumentNullException(nameof(select));
            }

            var atoms = select
                .Split(',')
                .Select(e => e?.Trim())
                .Where(e => !string.IsNullOrEmpty(e))
                .Select(s => SelectAggregationAtom.Parse(s));

            return new SelectAggregationExpression(atoms);
        }
    }
}
