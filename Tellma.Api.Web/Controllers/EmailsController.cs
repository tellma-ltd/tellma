using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/emails")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class EmailsController : FactGetByIdControllerBase<EmailForQuery, int, EntitiesResult<EmailForQuery>, EmailResult>
    {
        private readonly EmailsService _service;

        public EmailsController(EmailsService service)
        {
            _service = service;
        }

        protected override FactGetByIdServiceBase<EmailForQuery, int, EntitiesResult<EmailForQuery>, EmailResult> GetFactGetByIdService()
        {
            return GetService();
        }

        private EmailsService GetService()
        {
            _service.SetIncludeBody(IncludeBody());
            return _service;
        }

        [HttpGet("{emailId}/attachments/{attachmentId}")]
        public async Task<ActionResult> GetAttachment(int emailId, int attachmentId, CancellationToken cancellation)
        {
            var result = await GetService().GetAttachment(emailId, attachmentId, cancellation);
            var contentType = ControllerUtilities.ContentType(result.FileName);

            return File(fileContents: result.FileBytes, contentType: contentType, result.FileName);
        }

        private bool IncludeBody()
        {
            const string paramName = "includeBody";

            return Request.Query.TryGetValue(paramName, out StringValues value)
                && value.FirstOrDefault()?.ToLower() == "true";
        }

        protected override Extras CreateExtras(EmailResult result)
        {
            var body = result.Body;
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }
            else
            {
                return new Extras
                {
                    ["Body"] = body
                };
            }
        }
    }
}
