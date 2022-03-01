using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Services.ApiAuthentication;

namespace Tellma.Controllers
{
    /// <summary>
    /// Implementation of <see cref="IServiceContextAccessor"/> that extracts 
    /// the information from the current HTTP web request.
    /// </summary>
    public class WebServiceContextAccessor : IServiceContextAccessor
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IExternalUserAccessor _exUserAccessor;

        public WebServiceContextAccessor(IHttpContextAccessor accessor, IExternalUserAccessor exUserAccessor)
        {
            _accessor = accessor;
            _exUserAccessor = exUserAccessor;
        }

        public bool IsAnonymous => false;

        public bool IsServiceAccount => _exUserAccessor.IsServiceAccount;

        public string ExternalUserId => _exUserAccessor.UserId;

        public string ExternalEmail => _exUserAccessor.Email;

        public string ExternalClientId => _exUserAccessor.ClientId;

        public int? TenantId => TenantIdImpl();

        public bool IsSilent => IsSilentImpl();

        public DateTime Today => TodayImpl();

        public string Calendar => CalendarImpl();

        #region Helpers

        private HttpContext Context => _accessor?.HttpContext;

        private int? TenantIdImpl()
        {
            const string headerName = "X-Tenant-Id";

            string tenantIdString;

            var headers = Context?.Request?.Headers;
            if (headers != null && headers.TryGetValue(headerName, out StringValues value))
            {
                tenantIdString = value.First();
                if (int.TryParse(tenantIdString, out int tenantId))
                {
                    return tenantId;
                }
                else
                {
                    throw new ServiceException($"The header '{headerName}' contains a value '{tenantIdString}' which could not be interpreted as an integer.");
                }
            }
            else
            {
                return null;
            }
        }

        private bool IsSilentImpl()
        {
            const string paramName = "silent";

            var query = Context?.Request?.Query;

            return query != null && 
                query.TryGetValue(paramName, out StringValues value) && 
                value.First().ToString().ToLower() == "true";
        }

        private DateTime TodayImpl()
        {
            const string headerName = "X-Today";

            var headers = Context?.Request?.Headers;
            if (headers != null && headers.TryGetValue(headerName, out StringValues value))
            {
                string todayString = value.First();
                if (DateTime.TryParse(todayString, out DateTime today))
                {
                    return today;
                }
                else
                {
                    throw new ServiceException($"The header '{headerName}' contains a value '{todayString}' which could not be interpreted as a date.");
                }
            }
            else
            {
                return DateTime.Today;
            }
        }

        private string CalendarImpl()
        {
            const string headerName = "X-Calendar";

            var headers = Context?.Request?.Headers;
            if (headers != null && headers.TryGetValue(headerName, out StringValues value))
            {
                return value.First();
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
