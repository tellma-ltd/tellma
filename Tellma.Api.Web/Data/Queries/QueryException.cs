using System;

namespace Tellma.Data.Queries
{
    public class QueryException : Exception
    {
        public QueryException(string msg) : base(msg)
        {
        }
    }
}
