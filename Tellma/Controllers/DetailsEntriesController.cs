using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
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

        private readonly DetailsEntriesService _service;

        public DetailsEntriesController(DetailsEntriesService service, ILogger<DetailsEntriesController> logger) : base(logger)
        {
            _service = service;
        }

        protected override FactWithIdServiceBase<DetailsEntry, int> GetFactWithIdService()
        {
            return _service;
        }
    }

    public class DetailsEntriesService : FactWithIdServiceBase<DetailsEntry, int>
    {
        private readonly ApplicationRepository _repo;

        private string View => DetailsEntriesController.BASE_ADDRESS;

        public DetailsEntriesService(IServiceProvider sp, ApplicationRepository repo) : base(sp)
        {
            _repo = repo;
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return await _repo.UserPermissions(action, View, cancellation);
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
