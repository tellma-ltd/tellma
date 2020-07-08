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
using Microsoft.AspNetCore.Http;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class SummaryEntriesController : FactControllerBase<SummaryEntry>
    {
        public const string BASE_ADDRESS = "summary-entries";

        private readonly SummaryEntriesService _service;

        public SummaryEntriesController(SummaryEntriesService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        protected override FactServiceBase<SummaryEntry> GetFactService()
        {
            return _service;
        }
    }

    public class SummaryEntriesService : FactServiceBase<SummaryEntry>
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ApplicationRepository _repo;
        private readonly ISettingsCache _settingsCache;

        private string View => SummaryEntriesController.BASE_ADDRESS;

        private readonly Dictionary<string, object> _parameterOverride = new Dictionary<string, object>();

        public void SetParameter(string key, object value)
        {
            _parameterOverride.Add(key, value);
        }

        public bool ClearParameter(string key)
        {
            return _parameterOverride.Remove(key);
        }

        public SummaryEntriesService(
            IHttpContextAccessor contextAccessor,
            ApplicationRepository repo,
            ISettingsCache settingsCache,
            IServiceProvider sp) : base(sp)
        {
            _contextAccessor = contextAccessor;
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
            IQueryCollection query = _contextAccessor.HttpContext?.Request?.Query;            

            if (_parameterOverride.TryGetValue(key, out object dateObj))
            {
                if (dateObj is DateTime castDate)
                {
                    date = castDate;
                }
                else
                {
                    // Programmer mistake
                    throw new BadRequestException($"Bug: The parameter {key} must be a {nameof(DateTime)} object");
                }
            }
            else if (query != null && query.ContainsKey(key))
            {
                string dateString = query[key].FirstOrDefault();
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
