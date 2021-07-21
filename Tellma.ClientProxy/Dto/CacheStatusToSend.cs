namespace Tellma.Controllers.Dto
{
    /// <summary>
    /// When one of the caches on the client side becomes invalid.
    /// </summary>
    public class CacheStatusToSend : TenantStatusToSend
    {
        /// <summary>
        /// Which cache to invalidate, the supported values are listed in <see cref="CacheTypes"/>.
        /// </summary>
        public string Type { get; set; }
    }
}
