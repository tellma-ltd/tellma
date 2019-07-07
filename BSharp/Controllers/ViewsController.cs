using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.ImportExport;
using BSharp.Services.MultiTenancy;
using BSharp.Services.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BSharp.Controllers
{
    [Route("api/views")]
    [LoadTenantInfo]
    public class ViewsController : ReadControllerBase<View, ViewForQuery, string>
    {
        private readonly ApplicationContext _db;
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly ILogger<ViewsController> _logger;
        private readonly IStringLocalizer<ViewsController> _localizer;

        private readonly ITenantUserInfoAccessor _tenantInfo;

        public ViewsController(ApplicationContext db, IModelMetadataProvider metadataProvider, ILogger<ViewsController> logger,
            IStringLocalizer<ViewsController> localizer, IServiceProvider serviceProvider, ITenantUserInfoAccessor tenantInfoAccessor) : base(logger, localizer, serviceProvider)
        {
            _db = db;
            _metadataProvider = metadataProvider;
            _logger = logger;
            _localizer = localizer;

            _tenantInfo = tenantInfoAccessor;
        }

        protected override AbstractDataGrid DtosToAbstractGrid(GetResponse<View> response, ExportArguments args)
        {
            throw new NotImplementedException();
        }

        protected override DbContext GetDbContext()
        {
            return _db;
        }

        protected override Func<Type, string> GetSources()
        {
            var info = _tenantInfo.GetCurrentInfo();
            return ControllerUtilities.GetApplicationSources(_localizer, info.PrimaryLanguageId, info.SecondaryLanguageId, info.TernaryLanguageId);
        }

        protected override ODataQuery<ViewForQuery, string> Search(ODataQuery<ViewForQuery, string> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(MeasurementUnitForQuery.Name);
                var name2 = nameof(MeasurementUnitForQuery.Name2);
                // var name3 = nameof(MeasurementUnitForQuery.Name3); // TODO
                var code = nameof(MeasurementUnitForQuery.Code);

                query.Filter($"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'");
            }

            return query;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(PermissionLevel level)
        {
            return ControllerUtilities.GetPermissions(_db.AbstractPermissions, level, "views");
        }
    }
}
