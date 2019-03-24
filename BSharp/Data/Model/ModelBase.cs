namespace BSharp.Data.Model
{
    /// <summary>
    /// Only for type-safety during development, i.e to prevent the silly 
    /// mistake of passing model entities as DTO entities
    /// </summary>
    public abstract class ModelBase
    {
        public static readonly string TenantId = nameof(TenantId);
    }
}
