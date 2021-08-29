using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Services.Utilities;
using Tellma.Utilities.Common;

namespace Tellma.Api.Web.Controllers
{
    public class ExceptionsFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionsFilter> _logger;
        private readonly IServiceContextAccessor _accessor;
        private readonly IStringLocalizer<Strings> _localizer;

        public ExceptionsFilter(ILogger<ExceptionsFilter> logger, IServiceContextAccessor accessor, IStringLocalizer<Strings> localizer)
        {
            _logger = logger;
            _accessor = accessor;
            _localizer = localizer;
        }

        public void OnException(ExceptionContext context)
        {
            var ex = context.Exception;
            IActionResult result;

            if (ex is TaskCanceledException)
            {
                // It doesn't matter what we return here since the client is not interested anymore
                result = new BadRequestResult();
            }
            else if (ex is ForbiddenException exf)
            {
                // The user is 
                if (exf.NotMember)
                {
                    var headers = context.HttpContext.Response.Headers;

                    // Admin
                    headers.Set("x-admin-settings-version", Constants.Unauthorized);
                    headers.Set("x-admin-permissions-version", Constants.Unauthorized);
                    headers.Set("x-admin-user-settings-version", Constants.Unauthorized);

                    // Application
                    headers.Set("x-settings-version", Constants.Unauthorized);
                    headers.Set("x-definitions-version", Constants.Unauthorized);
                    headers.Set("x-permissions-version", Constants.Unauthorized);
                    headers.Set("x-user-settings-version", Constants.Unauthorized);
                }

                result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
            }
            else if (ex is NotFoundException exnf)
            {
                result = new NotFoundObjectResult(exnf.GetIds());
            }
            else if (ex is ValidationException exv)
            {
                result = new UnprocessableEntityObjectResult(ToModelState(exv.ModelState));
            }
            else if (ex is ReportableException)
            {
                // Any exception inheriting from this can be safely reported to the client
                result = new BadRequestObjectResult(ex.Message);
            }
            else if (ex is System.Data.SqlClient.SqlException sx && sx.Number == 4060)
            {
                // A DB in single user mode returns 4060 error.
                result = new BadRequestObjectResult(_localizer["Error_SystemUnderMaintenance"]);
            }
            else
            {
                var request = context.HttpContext.Request;

                // Collect as much information as possible
                string errorMessage = @$"Request Details:
- User Email: {_accessor.ExternalEmail ?? "-"}
- User Id: {_accessor.ExternalUserId ?? "-"}
- Client Id: {_accessor.ExternalClientId}
- Tenant Id: {_accessor.TenantId?.ToString() ?? "-"}
- Request: {request.Method} {request.Path}{request.QueryString}
- Request Identifier: {context.HttpContext.TraceIdentifier}";

                _logger.LogError(ex, errorMessage);

                //// Return 500 Internal Server Error
                //result = new ObjectResult(new { context.HttpContext.TraceIdentifier })
                //{
                //    StatusCode = (int)HttpStatusCode.InternalServerError
                //};

                // TODO: Hide the server error from the response and rely on the log to debug it
                result = new BadRequestObjectResult(ex.Message);
            }

            context.Result = result;
        }

        /// <summary>
        /// Transforms a service layer <see cref="ValidationErrorsDictionary"/> to the web's <see cref="ModelStateDictionary"/>.
        /// </summary>
        private static ModelStateDictionary ToModelState(ValidationErrorsDictionary validationErrors)
        {
            var result = new ModelStateDictionary();
            foreach (var (key, errors) in validationErrors.AllErrors)
            {
                foreach (var error in errors)
                {
                    result.AddModelError(key, error);
                }
            }

            return result;
        }
    }
}
