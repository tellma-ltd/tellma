using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.OData
{
    public class SqlJoinClause
    {
        public SqlJoinClause(JoinTree tree)
        {
            JoinTree = tree;
        }

        public JoinTree JoinTree { get; private set; }

        public string ToSql(Func<Type, string> sources)
        {
            return JoinTree.GetSql(sources);
        }
    }
}
