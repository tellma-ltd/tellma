using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

        public ExceptionsFilter(ILogger<ExceptionsFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var ex = context.Exception;
            IActionResult result;

            if (ex is TaskCanceledException)
            {
                // It doesn't matter what we return here since the client is not interested naymore
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
            else
            {
                _logger.LogError(ex, "An unhandled exception has occurred while executing an API request.");
                var objectResult = new ObjectResult(context.HttpContext.TraceIdentifier)
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };

                result = objectResult;
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
