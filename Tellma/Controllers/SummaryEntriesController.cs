using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
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

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return await _repo.UserPermissions(action, View, cancellation);
        }

        protected override IRepository GetRepository()
        {
            return new ParameteredRepository<SummaryEntry>(_repo,
                ("@FromDate", GetDate("FromDate", true)),
                ("@ToDate", GetDate("ToDate", true))
            );
        }

        private DateTime? GetDate(string key, bool isRequired)
        {
            DateTime? date = null;
            if (Request.Query.ContainsKey(key))
            {
                string dateString = Request.Query[key].FirstOrDefault();
                try
                {
                    date = DateTime.Parse(dateString);
                }
                catch
                {
                    throw new BadRequestException($"Failed to convert {key}: '{dateString}' to a valid DateTime value");
                }
            }
            else if (isRequired)
            {
                throw new BadRequestException($"The parameter {key} is required");
            }

            return date;
        }

        protected override Query<SummaryEntry> Search(Query<SummaryEntry> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            //string search = args.Search;
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = search.Replace("'", "''"); // escape quotes by repeating them

            //    var name = nameof(SummaryEntry.Name);

            //    query = query.Filter($"{name} {Ops.contains} '{search}'");
            //}

            return query;
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse(nameof(SummaryEntry.AccountId));
        }
    }
}
