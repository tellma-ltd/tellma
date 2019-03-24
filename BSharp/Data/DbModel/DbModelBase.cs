namespace BSharp.Data.DbModel
{
    /// <summary>
    /// Only for type-safety during development, i.e to prevent the silly 
    /// mistake of passing model entities as DTO entities
    /// </summary>
    public abstract class DbModelBase
    {
        public static readonly string TenantId = nameof(TenantId);
    }
}
