using BSharp.Controllers.DTO;
using BSharp.Services.MultiTenancy;
using Microsoft.Extensions.Localization;
using System;
using System.Data.Common;

namespace BSharp.Services.OData
{
    public class ODataQueryFactory : IODataQueryFactory
    {
        private readonly IStringLocalizer<ODataQueryFactory> _localizer;
        private readonly ITenantUserInfoAccessor _accessor;

        public ODataQueryFactory(IStringLocalizer<ODataQueryFactory> localizer, ITenantUserInfoAccessor accessor)
        {
            _localizer = localizer;
            _accessor = accessor;
        }

        public ODataQuery<T, TKey> MakeODataQuery<T, TKey>(DbConnection conn, Func<Type, string> sources) where T : DtoKeyBase<TKey>
        {
            var timeZone = TimeZoneInfo.Local; // TODO: pick up from the context
            var userId = _accessor?.GetCurrentInfo()?.UserId ?? 0;

            return new ODataQuery<T, TKey>(conn, sources, _localizer, userId, timeZone); // TODO: handle the case when we are not querying tenant data
        }
    }
}
