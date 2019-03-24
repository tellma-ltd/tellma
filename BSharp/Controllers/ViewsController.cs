using AutoMapper;
using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.Identity;
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
using M = BSharp.Data.DbModel;

namespace BSharp.Controllers
{
    [Route("api/views")]
    [LoadTenantInfo]
    public class ViewsController : CrudControllerBase<ViewDefinition, View, ViewForSave, string>
    {
        private readonly ApplicationContext _db;
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly ILogger<ViewsController> _logger;
        private readonly IStringLocalizer<ViewsController> _localizer;
        private readonly IMapper _mapper;
        private readonly ITenantUserInfoAccessor _accessor;

        public ViewsController(ApplicationContext db, IModelMetadataProvider metadataProvider, ILogger<ViewsController> logger,
            IStringLocalizer<ViewsController> localizer, IMapper mapper, ITenantUserInfoAccessor accessor) : base(logger, localizer, mapper)
        {
            _db = db;
            _metadataProvider = metadataProvider;
            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
            _accessor = accessor;
        }

        protected override async Task<IDbContextTransaction> BeginSaveTransaction()
        {
            return await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        }

        protected override IQueryable<ViewDefinition> GetBaseQuery()
        {
            var repo = new ViewsRepository(_db, _localizer, _accessor);
            return repo.GetAllViews().AsQueryable();
        }

        protected override IQueryable<ViewDefinition> SingletonQuery(IQueryable<ViewDefinition> query, string id)
        {
            return query.Where(e => e.Id == id);
        }

        protected override IQueryable<ViewDefinition> Search(IQueryable<ViewDefinition> query, string search)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Name.ToLower().Contains(search.ToLower()) || (e.Name != null && e.Name2.ToLower().Contains(search.ToLower())));
            }

            return query;
        }

        protected override IQueryable<ViewDefinition> Expand(IQueryable<ViewDefinition> query, string expand)
        {
            if (expand != null)
            {
                // var expands = expand.Split(',');
                // TODO
            }

            return query;
        }

        protected override IQueryable<ViewDefinition> IncludeInactive(IQueryable<ViewDefinition> query, bool inactive)
        {
            if (!inactive)
            {
                query = query.Where(e => e.IsActive);
            }

            return query;
        }

        protected override Task ValidateAsync(List<ViewForSave> entities)
        {
            throw new NotImplementedException();
        }

        protected override Task<List<ViewDefinition>> PersistAsync(List<ViewForSave> entities, SaveArguments args)
        {
            throw new NotImplementedException();
        }

        protected override Task DeleteAsync(List<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override AbstractDataGrid DtosToAbstractGrid(GetResponse<View> response, ExportArguments args)
        {
            throw new NotImplementedException();
        }

        protected override AbstractDataGrid GetImportTemplate()
        {
            // TODO
            throw new NotImplementedException();
        }

        protected override Task<(List<ViewForSave>, Func<string, int?>)> ToDtosForSave(AbstractDataGrid grid, ParseArguments args)
        {
            throw new NotImplementedException();
        }

        protected override Task<IEnumerable<M.AbstractPermission>> UserPermissions(PermissionLevel level)
        {
            return GetPermissions(_db.AbstractPermissions, level, "views");
        }

        protected override Task CheckPermissionsForNew(IEnumerable<ViewForSave> newItems, Expression<Func<ViewDefinition, bool>> lambda)
        {
            throw new NotImplementedException();
        }

        protected override Task CheckPermissionsForOld(IEnumerable<string> entityIds, Expression<Func<ViewDefinition, bool>> lambda)
        {
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
            if(tenantInfo.SecondaryLanguageId != null)
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

    public class ViewDefinition : M.DbModelBase
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public string AllowedPermissionLevels { get; set; } = "";
    }

}