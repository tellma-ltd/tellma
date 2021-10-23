using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Application;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    [Route("api/notification-templates")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class NotificationTemplatesController : CrudControllerBase<NotificationTemplateForSave, NotificationTemplate, int>
    {
        private readonly NotificationTemplatesService _service;

        public NotificationTemplatesController(NotificationTemplatesService service)
        {
            _service = service;
        }


        [HttpGet("preview-email/{templateId}")]
        public async Task<ActionResult> Preview(int templateId, [FromQuery] PrintEntitiesArguments<int> args, CancellationToken cancellation)
        {
            args.Custom = Request.Query.ToDictionary(e => e.Key, e => e.Value.FirstOrDefault());

            var result = await _service.PreviewEmailEntities(templateId, args, cancellation);

            return Ok(result);

            //var fileBytes = result.FileBytes;
            //var fileName = result.FileName;
            //var contentType = ControllerUtilities.ContentType(fileName);
            //Response.Headers.Add("x-filename", fileName);

            //return File(fileContents: fileBytes, contentType: contentType, fileName);
        }


        protected override CrudServiceBase<NotificationTemplateForSave, NotificationTemplate, int> GetCrudService()
        {
            return _service;
        }

        protected override Task OnSuccessfulSave(EntitiesResult<NotificationTemplate> data)
        {
            if (data?.Data != null && data.Data.Any(e => e.IsDeployed ?? false))
            {
                Response.Headers.Set("x-definitions-version", Constants.Stale);
            }

            return base.OnSuccessfulSave(data);
        }
    }
}
