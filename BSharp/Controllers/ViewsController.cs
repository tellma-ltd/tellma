using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.ImportExport;
using BSharp.Services.MultiTenancy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using M = BSharp.Data.Model;

namespace BSharp.Controllers
{
    [Route("api/views")]
    [LoadTenantInfo]
    public class ViewsController : CrudControllerBase<ViewForSave, View, ViewForQuery, string>
    {
        private readonly ApplicationContext _db;
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly ILogger<ViewsController> _logger;
        private readonly IStringLocalizer<ViewsController> _localizer;

        private readonly ITenantUserInfoAccessor _accessor;

        public ViewsController(ApplicationContext db, IModelMetadataProvider metadataProvider, ILogger<ViewsController> logger,
            IStringLocalizer<ViewsController> localizer, IServiceProvider serviceProvider, ITenantUserInfoAccessor accessor) : base(logger, localizer, serviceProvider)
        {
            _db = db;
            _metadataProvider = metadataProvider;
            _logger = logger;
            _localizer = localizer;

            _accessor = accessor;
        }

        protected override async Task<IDbContextTransaction> BeginSaveTransaction()
        {
            return await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(PermissionLevel level)
        {
            return ControllerUtilities.GetPermissions(_db.AbstractPermissions, level, "views");
        }

        protected override Task CheckPermissionsForNew(IEnumerable<ViewForSave> newItems, Expression<Func<ViewForQuery, bool>> lambda)
        {
            // TODO
            throw new NotImplementedException();
        }

        protected override Task CheckPermissionsForOld(IEnumerable<string> entityIds, Expression<Func<ViewForQuery, bool>> lambda)
        {
            // TODO
            throw new NotImplementedException();
        }

        protected override IQueryable<ViewForQuery> GetBaseQuery()
        {
            return _db.VW_Views;
        }

        protected override IQueryable<ViewForQuery> IncludeInactive(IQueryable<ViewForQuery> query, bool inactive)
        {
            if (!inactive)
            {
                query = query.Where(e => e.IsActive == true);
            }

            return query;
        }

        protected override IQueryable<ViewForQuery> Search(IQueryable<ViewForQuery> query, string search, IEnumerable<AbstractPermission> filteredPermissions)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => 
                    (e.Name != null && e.Name.ToLower().Contains(search.ToLower()) == true) ||
                    (e.Name2 != null && e.Name2.ToLower().Contains(search.ToLower()) == true) ||
                    (e.ResourceName != null && _localizer[e.ResourceName].Value.ToLower().Contains(search.ToLower()))
                );
            }

            return query;
        }

        protected override Task ValidateAsync(List<ViewForSave> entities)
        {
            // TODO
            throw new NotImplementedException();
        }

        protected override Task<(List<ViewForQuery>, IQueryable<ViewForQuery>)> PersistAsync(List<ViewForSave> entities, SaveArguments args)
        {
            // TODO
            throw new NotImplementedException();
        }

        protected override Task DeleteAsync(List<string> ids)
        {
            // TODO
            throw new NotImplementedException();
        }

        protected override AbstractDataGrid DtosToAbstractGrid(GetResponse<View> response, ExportArguments args)
        {
            // TODO
            throw new NotImplementedException();
        }

        protected override AbstractDataGrid GetImportTemplate()
        {
            // TODO
            throw new NotImplementedException();
        }

        protected override Task<(List<ViewForSave>, Func<string, int?>)> ToDtosForSave(AbstractDataGrid grid, ParseArguments args)
        {
            // TODO
            throw new NotImplementedException();
        }
    }

    public class ViewsRepository
    {
        private static readonly object _staticViewsLock = new object();
        private static List<StaticViewDefinition> _staticViews;
        private readonly ApplicationContext _db;
        private readonly IStringLocalizer _localizer;
        private readonly IStringLocalizer _localizer2;

        public ViewsRepository(ApplicationContext db, IStringLocalizer localizer, ITenantUserInfoAccessor accessor)
        {
            _db = db;

            // Initialize localizer 1 and localizer 2 based on the company languages
            var tenantInfo = accessor.GetCurrentInfo();
            _localizer = localizer.WithCulture(new CultureInfo(tenantInfo.PrimaryLanguageId));
            if (tenantInfo.SecondaryLanguageId != null)
            {
                _localizer2 = localizer.WithCulture(new CultureInfo(tenantInfo.SecondaryLanguageId)); // TODO
            }
        }

        public static IEnumerable<StaticViewDefinition> GetAllStaticViews()
        {
            if (_staticViews == null)
            {
                lock (_staticViewsLock)
                {
                    if (_staticViews == null)
                    {
                        _staticViews = new[] {
                            new StaticViewDefinition(name: "View_All", code: "all", levels: "ReadUpdate"),
                            new StaticViewDefinition(name: "MeasurementUnits", code: "measurement-units", levels: "ReadUpdate"),
                            new StaticViewDefinition(name: "Roles", code: "roles", levels: "ReadUpdate"),
                            new StaticViewDefinition(name: "Users", code: "local-users", levels: "ReadUpdate"),
                            new StaticViewDefinition(name: "Views", code: "views", levels: "Read"),
                            new StaticViewDefinition(name: "Individuals", code: "individuals", levels: "ReadUpdate"),
                            new StaticViewDefinition(name: "Organizations", code: "organizations", levels: "ReadUpdate"),
                            new StaticViewDefinition(name: "Settings", code: "settings", levels: "ReadUpdate")
                        }.ToList();
                    }
                }
            }

            return _staticViews;
        }

        public IEnumerable<ViewDefinition> GetAllViews()
        {
            var staticViews = GetAllStaticViews();
            var dynamicViews = new List<DynamicViewDefinition>(); // TODO
            var activeViewCodes = _db.Views.Where(e => e.IsActive).Select(e => e.Id).ToHashSet();

            var allViews = new List<ViewDefinition>();
            foreach (var view in staticViews)
            {
                allViews.Add(new ViewDefinition
                {
                    Id = view.Code,
                    Name = _localizer[view.Name],
                    Name2 = _localizer2 != null ? _localizer2[view.Name] : "",
                    IsActive = activeViewCodes.Contains(view.Code) || view.Code == "all",
                    AllowedPermissionLevels = view.AllowedPermissionLevels
                });
            }

            foreach (var view in dynamicViews)
            {
                allViews.Add(new ViewDefinition
                {
                    Id = view.Code,
                    Name = view.Name,
                    Name2 = view.Name2,
                    IsActive = activeViewCodes.Contains(view.Code),
                    AllowedPermissionLevels = view.AllowedPermissionLevels
                });
            }

            return allViews;
        }
    }

    public class StaticViewDefinition
    {
        public string Name { get; private set; }
        public string Code { get; private set; }
        public string AllowedPermissionLevels { get; private set; } = "";

        public StaticViewDefinition(string name, string code, string levels)
        {
            Name = name;
            Code = code;
            AllowedPermissionLevels = levels;
        }
    }

    public class DynamicViewDefinition
    {
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string Code { get; set; }
        public string AllowedPermissionLevels { get; set; } = "";
    }

    public class ViewDefinition : M.ModelBase
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public string AllowedPermissionLevels { get; set; } = "";
    }
}
