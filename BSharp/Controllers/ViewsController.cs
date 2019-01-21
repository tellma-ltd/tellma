using AutoMapper;
using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.ImportExport;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using M = BSharp.Data.Model;

namespace BSharp.Controllers
{
    [Route("api/views")]
    public class ViewsController : CrudControllerBase<M.View, View, ViewForSave, string>
    {
        private readonly ApplicationContext _db;
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly ILogger<ViewsController> _logger;
        private readonly IStringLocalizer<ViewsController> _localizer;
        private readonly IMapper _mapper;

        public ViewsController(ApplicationContext db, IModelMetadataProvider metadataProvider, ILogger<ViewsController> logger,
            IStringLocalizer<ViewsController> localizer, IMapper mapper) : base(logger, localizer, mapper)
        {
            _db = db;
            _metadataProvider = metadataProvider;
            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
        }

        protected override async Task<IDbContextTransaction> BeginSaveTransaction()
        {
            return await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        }

        public IEnumerable<View> GetAllViews()
        {
            return new List<View>();
        }

        protected override IQueryable<M.View> GetBaseQuery()
        {
            return _db.Views;
        }

        protected override IQueryable<M.View> SingletonQuery(IQueryable<M.View> query, string id)
        {
            return query.Where(e => e.Id == id);
        }

        protected override IQueryable<M.View> Search(IQueryable<M.View> query, string search)
        {
            // TODO
            return query;
        }

        protected override IQueryable<M.View> IncludeInactive(IQueryable<M.View> query, bool inactive)
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

        protected override Task<List<M.View>> PersistAsync(List<ViewForSave> entities, SaveArguments args)
        {
            throw new NotImplementedException();
        }

        protected override Task DeleteImplAsync(List<string> ids)
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
    }
}