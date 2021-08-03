using Tellma.Utilities.Common;

namespace Tellma.Repository.Common.Queryex
{
    public class QueryException : ReportableException
    {
        public QueryException(string msg) : base(msg)
        {
        }
    }
}
