using BSharp.Controllers.Dto;
using BSharp.Controllers.Utilities;
using BSharp.Data;
using BSharp.Data.Queries;
using BSharp.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationApi]
    public class SummaryEntriesController : FactControllerBase<SummaryEntry>
    {
        public const string BASE_ADDRESS = "summary-entries";

        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;
        private readonly ISettingsCache _settingsCache;

        private string View => BASE_ADDRESS;

        public SummaryEntriesController(
            ILogger<SummaryEntriesController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo,
            ISettingsCache settingsCache) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
            _repo = repo;
            _settingsCache = settingsCache;
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return await _repo.UserPermissions(action, View);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<SummaryEntry> Search(Query<SummaryEntry> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(SummaryEntry.Name);

                query = query.Filter($"{name} {Ops.contains} '{search}'");
            }

            return query;
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse(nameof(SummaryEntry.Name));
        }
    }
}
