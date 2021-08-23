namespace Tellma.Repository.Common.Queryex
{
    /// <summary>
    /// Represents a NULL (no value). Results from any mention of the keyword "null".
    /// </summary>
    public class QueryexNull : QueryexBase
    {
        public override string ToString()
        {
            return "null";
        }

        public override (string, QxType, QxNullity) CompileNative(QxCompilationContext ctx)
        {
            return ("NULL", QxType.Null, QxNullity.Null);
        }

        public override QueryexBase Clone(string[] prefix = null) => new QueryexNull();

        public override bool Equals(object obj)
        {
            return obj is QueryexNull;
        }

        public override int GetHashCode()
        {
            return true.GetHashCode(); // Doesn't matter
        }
    }
}
