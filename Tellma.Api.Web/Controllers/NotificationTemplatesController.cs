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

        [HttpPut("email-entities-preview")]
        public async Task<ActionResult<PrintPreviewResponse>> EmailCommandPreviewEntities(
            [FromBody] NotificationTemplate template,
            [FromQuery] PrintEntitiesArguments<int> args,
            CancellationToken cancellation)
        {
            var result = await _service.EmailCommandPreviewEntities(template, args, cancellation);
            return Ok(result);
        }

        [HttpPut("email-entities-preview/{index:int}")]
        public async Task<ActionResult<PrintPreviewResponse>> EmailPreviewEntities(
            int index,
            [FromBody] NotificationTemplate template,
            [FromQuery] PrintEntitiesArguments<int> args,
            CancellationToken cancellation)
        {
            var result = await _service.EmailPreviewEntities(template, index, args, cancellation);
            return Ok(result);
        }

        //[HttpPut("email-entities-preview")]
        //public async Task<ActionResult<PrintPreviewResponse>> EmailPreviewEntities(
        //    [FromBody] NotificationTemplate template, [FromQuery] PrintEntitiesArguments<int> args, CancellationToken cancellation)
        //{
        //    var result = await _service.EmailPreviewEntities(template, 0, args, cancellation);
        //    return Ok(result);
        //}

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
