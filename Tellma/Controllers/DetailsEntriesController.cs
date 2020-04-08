using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class DetailsEntriesController : FactWithIdControllerBase<DetailsEntry, int>
    {
        public const string BASE_ADDRESS = "details-entries";

        private readonly ApplicationRepository _repo;

        private string View => BASE_ADDRESS;

        public DetailsEntriesController(
            ILogger<DetailsEntriesController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo) : base(logger, localizer)
        {
            _repo = repo;
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return await _repo.UserPermissions(action, View);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<DetailsEntry> Search(Query<DetailsEntry> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            return query;
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse(nameof(DetailsEntry.AccountId));
        }
    }
}
