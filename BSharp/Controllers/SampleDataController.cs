using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BSharp.Data;
using BSharp.Services.MultiTenancy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace BSharp.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {
        public SampleDataController(ApplicationContext ctx, IStringLocalizer<SampleDataController> localizer)
        {
            _ctx = ctx;
            _localizer = localizer;
        }

        private readonly ApplicationContext _ctx;
        private readonly IStringLocalizer<SampleDataController> _localizer;

        //[HttpGet("[action]")]
        //public IEnumerable<WeatherForecast> WeatherForecasts()
        //{
        //    var rng = new Random();
        //    return _ctx.Translations.Select(e => new WeatherForecast
        //    {
        //        DateFormatted = e.Name,
        //        TemperatureC = rng.Next(-20, 55),
        //        Summary = e.Value
        //    });
        //}

        [HttpGet("[action]")]
        public string WeatherForecasts()
        {
            var rng = new Random();
            return _localizer["BlueSky"];
        }

        public class WeatherForecast
        {
            public string DateFormatted { get; set; }
            public int TemperatureC { get; set; }
            public string Summary { get; set; }

            public int TemperatureF
            {
                get
                {
                    return 32 + (int)(TemperatureC / 0.5556);
                }
            }
        }
    }
}
