using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Services.OData
{
    public class GroupByExpression : IEnumerable<GroupByAtom>
    {
        private IEnumerable<GroupByAtom> _atoms;

        public GroupByExpression(IEnumerable<GroupByAtom> atoms)
        {
            _atoms = atoms ?? throw new ArgumentNullException(nameof(atoms));
        }

        public IEnumerator<GroupByAtom> GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }

        public static GroupByExpression Parse(string groupby)
        {
            if (string.IsNullOrWhiteSpace(groupby))
            {
                return null;
            }

            var atoms = groupby
                .Split(',')
                .Select(e => e?.Trim())
                .Where(e => !string.IsNullOrEmpty(e))
                .Select(s => GroupByAtom.Parse(s));

            return new GroupByExpression(atoms);
        }
    }
}
