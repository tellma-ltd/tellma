using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.ImportExport;
using BSharp.Services.MultiTenancy;
using BSharp.Services.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers
{
    // TODO remove
    [Route("api/measurement-units-fact")]
    [LoadTenantInfo]
    public class MeasurementUnitsFactController : ReadControllerBase<MeasurementUnitFact>
    {
        private readonly ApplicationContext _db;
        private readonly ITenantUserInfoAccessor _tenantInfo;
        private readonly IStringLocalizer _localizer;

        public MeasurementUnitsFactController(ILogger<MeasurementUnitsFactController> logger, IStringLocalizer<MeasurementUnitsFactController> localizer, 
            IServiceProvider serviceProvider) : base(logger, localizer, serviceProvider)
        {
            _db = serviceProvider.GetRequiredService<ApplicationContext>();
            _tenantInfo = serviceProvider.GetRequiredService<ITenantUserInfoAccessor>();
            _localizer = localizer;
        }

        protected override string DefaultOrderBy()
        {
            return "Code";
        }

        protected override AbstractDataGrid DtosToAbstractGrid(GetResponse<MeasurementUnitFact> response, ExportArguments args)
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

        protected override ODataQuery<MeasurementUnitFact> Search(ODataQuery<MeasurementUnitFact> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(MeasurementUnitFact.Name);
                var name2 = nameof(MeasurementUnitFact.Name2);
                // var name3 = nameof(MeasurementUnit.Name3); // TODO
                var code = nameof(MeasurementUnitFact.Code);

                query.Filter($"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'");
            }

            return query;
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(PermissionLevel level)
        {
            return await ControllerUtilities.GetPermissions(_db.AbstractPermissions, level, "measurement-units");
        }
    }
}
