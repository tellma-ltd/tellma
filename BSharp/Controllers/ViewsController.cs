using BSharp.Controllers.Dto;
using BSharp.Controllers.Utilities;
using BSharp.Data;
using BSharp.Data.Queries;
using BSharp.Entities;
using BSharp.Services.ImportExport;
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
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationApi]
    public class ViewsController : FactGetByIdControllerBase<View, string>
    {
        public const string BASE_ADDRESS = "views";

        private readonly ApplicationRepository _repo;
        private readonly Microsoft.AspNetCore.Mvc.ModelBinding.IModelMetadataProvider _metadataProvider;
        private readonly ILogger<ViewsController> _logger;
        private readonly IStringLocalizer _localizer;

        private string ViewId => BASE_ADDRESS;

        public ViewsController(ApplicationRepository repo, Microsoft.AspNetCore.Mvc.ModelBinding.IModelMetadataProvider metadataProvider, 
            ILogger<ViewsController> logger,
            IStringLocalizer<Strings> localizer) : base(logger, localizer)
        {
            _repo = repo;
            _metadataProvider = metadataProvider;
            _logger = logger;
            _localizer = localizer;
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<View> Search(Query<View> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(View.Name);
                var name2 = nameof(View.Name2);
                var name3 = nameof(View.Name3);
                var code = nameof(View.Code);
                var cs = Ops.contains;

                query = query.Filter($"{name} {cs} '{search}' or {name2} {cs} '{search}' or {name3} {cs} '{search}' or {code} {cs} '{search}'");
            }

            return query;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return _repo.UserPermissions(action, ViewId);
        }
    }
}
