using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Services.OData
{
    public class ExpandExpression : IEnumerable<ExpandAtom>
    {
        private List<ExpandAtom> _atoms;

        public ExpandExpression()
        {
            _atoms = new List<ExpandAtom>();
        }

        public ExpandExpression(IEnumerable<ExpandAtom> atoms)
        {
            _atoms = atoms.ToList() ?? throw new ArgumentNullException(nameof(atoms));
        }

        public void Add(ExpandAtom atom)
        {
            _atoms.Add(atom);
        }

        public IEnumerator<ExpandAtom> GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }

        public static ExpandExpression Parse(string expand)
        {
            if (string.IsNullOrWhiteSpace(expand))
            {
                return null;
            }

            var atoms = expand
                .Split(',')
                .Select(e => e?.Trim())
                .Where(e => !string.IsNullOrEmpty(e))
                .Select(s => ExpandAtom.Parse(s));

            return new ExpandExpression(atoms);
        }

        public static ExpandExpression Empty
        {
            get
            {
                return new ExpandExpression(new List<ExpandAtom>());
            }
        }

        public static ExpandExpression RootSingleton
        {
            get
            {
                return new ExpandExpression(new List<ExpandAtom> { new ExpandAtom { Path = new string[0] } });
            }
        }
    }
}
