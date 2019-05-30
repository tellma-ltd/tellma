using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Services.OData
{
    public class SelectExpression : IEnumerable<SelectAtom>
    {
        private List<SelectAtom> _atoms;

        public SelectExpression()
        {
            _atoms = new List<SelectAtom>();
        }

        public SelectExpression(IEnumerable<SelectAtom> atoms)
        {
            _atoms = atoms.ToList() ?? throw new ArgumentNullException(nameof(atoms));
        }

        public void Add(SelectAtom atom)
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

        public IEnumerator<SelectAtom> GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }

        public static SelectExpression Parse(string select)
        {
            if (string.IsNullOrWhiteSpace(select))
            {
                return null;
            }

            var atoms = select
                .Split(',')
                .Select(e => e?.Trim())
                .Where(e => !string.IsNullOrEmpty(e))
                .Select(s => SelectAtom.Parse(s));

            return new SelectExpression(atoms);
        }
    }
}
