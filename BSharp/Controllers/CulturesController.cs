//using BSharp.Controllers.Dto;
//using BSharp.Controllers.Misc;
//using BSharp.Data;
//using BSharp.Services.ApiAuthentication;
//using BSharp.Services.ImportExport;
//using BSharp.Services.MultiTenancy;
//using BSharp.Services.OData;
//using BSharp.Services.Utilities;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Localization;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;


//namespace BSharp.Controllers
//{
//    [Route("api/[controller]")]
//    [AuthorizeAccess]
//    [ApiController]
//    public class CulturesController : ReadEntitiesControllerBase<Culture, string>
//    {
//        private readonly AdminContext _db;
//        private readonly ILogger<CulturesController> _logger;
//        private readonly IStringLocalizer<CulturesController> _localizer;
//        private readonly ITenantUserInfoAccessor _tenantInfo;

//        public CulturesController(ILogger<CulturesController> logger, IStringLocalizer<CulturesController> localizer,
//            IServiceProvider serviceProvider) : base(logger, localizer, serviceProvider)
//        {
//            _db = serviceProvider.GetRequiredService<AdminContext>();
//            _logger = logger;
//            _localizer = localizer;

//            _tenantInfo = serviceProvider.GetRequiredService<ITenantUserInfoAccessor>();
//        }

//        protected override AbstractDataGrid EntitiesToAbstractGrid(GetResponse<Culture> response, ExportArguments args)
//        {
//            throw new NotImplementedException();
//        }

//        protected override DbContext GetRepository()
//        {
//            return _db;
//        }

//        protected override Func<Type, string> GetSources()
//        {
//            var info = _tenantInfo.GetCurrentInfo();
//            return ControllerUtilities.GetApplicationSources(_localizer, info.PrimaryLanguageId, info.SecondaryLanguageId, info.TernaryLanguageId);
//        }

//        protected override ODataQuery<Culture> Search(ODataQuery<Culture> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
//        {
//            string search = args.Search;
//            if (!string.IsNullOrWhiteSpace(search))
//            {
//                search = search.Replace("'", "''"); // escape quotes by repeating them

//                var name = nameof(Culture.Name);
//                var englishName = nameof(Culture.EnglishName);

//                query.Filter($"{name} {Ops.contains} '{search}' or {englishName} {Ops.contains} '{search}'");
//            }

//            return query;
//        }

//        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
//        {
//            // Cultures are always readable for all
//            IEnumerable<AbstractPermission> result = new List<AbstractPermission>
//            {
//                new AbstractPermission { ViewId = "cultures", Action = Constants.Update }
//            };

//            return Task.FromResult(result);
//        }
//    }
//}
