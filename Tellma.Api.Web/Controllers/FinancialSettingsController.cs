using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using Tellma.Api;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/financial-settings")]
    [ApiVersion("1.0")]
    public class FinancialSettingsController : ApplicationSettingsControllerBase<FinancialSettingsForSave, FinancialSettings>
    {
        private readonly FinancialSettingsService _service;

        public FinancialSettingsController(IServiceProvider sp, FinancialSettingsService service) : base(sp)
        {
            _service = service;
        }

        protected override ApplicationSettingsServiceBase<FinancialSettingsForSave, FinancialSettings> GetSettingsService()
        {
            return _service;
        }
    }
}
