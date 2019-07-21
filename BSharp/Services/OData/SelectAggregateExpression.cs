using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.OData
{
    public class SelectAggregateExpression : IEnumerable<SelectAggregateAtom>
    {
        private List<SelectAggregateAtom> _atoms;

        public SelectAggregateExpression()
        {
            _atoms = new List<SelectAggregateAtom>();
        }

        public SelectAggregateExpression(IEnumerable<SelectAggregateAtom> atoms)
        {
            _atoms = atoms.ToList() ?? throw new ArgumentNullException(nameof(atoms));
        }

        public void Add(SelectAggregateAtom atom)
        {
            _atoms.Add(atom);
        }

        public int Count
        {
            get
            {
                return _atoms.Count;
            }
        }

        public IEnumerator<SelectAggregateAtom> GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }

        public static SelectAggregateExpression Parse(string select)
        {
            if (string.IsNullOrWhiteSpace(select))
            {
                return null;
            }

            var atoms = select
                .Split(',')
                .Select(e => e?.Trim())
                .Where(e => !string.IsNullOrEmpty(e))
                .Select(s => SelectAggregateAtom.Parse(s));

            return new SelectAggregateExpression(atoms);
        }
    }
}
