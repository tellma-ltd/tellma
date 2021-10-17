using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Api.Dto;
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
