namespace BSharp.Data.DbModel
{
    /// <summary>
    /// A Query type https://docs.microsoft.com/en-us/ef/core/modeling/query-types
    /// Used for returning a flat list of strings
    /// </summary>
    public class DbString
    {
        public string Value { get; set; }
    }
}
