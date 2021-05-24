using System;

namespace Tellma.Repository.Common.Queryex
{
    public class QueryException : Exception
    {
        public QueryException(string msg) : base(msg)
        {
        }
    }
}
