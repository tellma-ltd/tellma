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

        public ODataQuery<T> MakeODataQuery<T>(DbConnection conn, Func<Type, string> sources) where T : DtoBase
        {
            var timeZone = TimeZoneInfo.Local; // TODO: pick up from the context
            var userId = _accessor?.GetCurrentInfo()?.UserId ?? 0;

            return new ODataQuery<T>(conn, sources, _localizer, userId, timeZone); // TODO: handle the case when we are not querying tenant data
        }

        public ODataAggregateQuery<T> MakeODataAggregateQuery<T>(DbConnection conn, Func<Type, string> sources) where T : DtoBase
        { 
            var timeZone = TimeZoneInfo.Local; // TODO: pick up from the context
            var userId = _accessor?.GetCurrentInfo()?.UserId ?? 0;

            return new ODataAggregateQuery<T>(conn, sources, _localizer, userId, timeZone); // TODO: handle the case when we are not querying tenant data
        }
    }
}
