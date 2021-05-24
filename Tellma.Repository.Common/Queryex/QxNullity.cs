namespace Tellma.Repository.Common.Queryex
{
    public enum QxNullity
    {
        // The values are chosen such that the bitwise operators & and | can be useful
        NotNull = 1,    // 001
        Nullable = 3,   // 011
        Null = 7        // 111
    }
}
