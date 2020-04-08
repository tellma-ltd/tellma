using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.MultiTenancy;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController(allowUnobtrusive: true)]
    public class InboxController : FactWithIdControllerBase<InboxRecord, int>
    {
        public const string BASE_ADDRESS = "inbox";
        private readonly ApplicationRepository _repo;
        private readonly ITenantIdAccessor _tenantIdAccessor;
        private readonly IHubContext<ServerNotificationsHub, INotifiedClient> _hubContext;
        private readonly ILogger<InboxController> _logger;

        public InboxController(
            ApplicationRepository repo,
            ITenantIdAccessor tenantIdAccessor,
            IHubContext<ServerNotificationsHub,
            INotifiedClient> hubContext,
            ILogger<InboxController> logger,
            IStringLocalizer<Strings> localizer) : base(logger, localizer)
        {
            _repo = repo;
            _tenantIdAccessor = tenantIdAccessor;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpPut("check")]
        public async Task<ActionResult> CheckInbox([FromBody] DateTimeOffset now)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var infos = await _repo.Inbox__Check(now);

                // Notify the user
                var tenantId = _tenantIdAccessor.GetTenantId();
                await _hubContext.NotifyInboxAsync(tenantId, infos, updateInboxList: false);

                return Ok();
            }
            , _logger);
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse("CreatedAt desc");
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<InboxRecord> Search(Query<InboxRecord> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var createdByProp = nameof(InboxRecord.CreatedBy);
                var nameProp = $"{createdByProp}/{nameof(Entities.User.Name)}";
                var name2Prop = $"{createdByProp}/{nameof(Entities.User.Name2)}";
                var name3Prop = $"{createdByProp}/{nameof(Entities.User.Name3)}";

                var commentProp = nameof(InboxRecord.Comment);
                var memoProp = $"{nameof(InboxRecord.Document)}/{nameof(Document.Memo)}";

                // Prepare the filter string
                var filterString = $"{nameProp} {Ops.contains} '{search}' or {name2Prop} {Ops.contains} '{search}' or {name3Prop} {Ops.contains} '{search}' or {commentProp} {Ops.contains} '{search}' or {memoProp} {Ops.contains} '{search}'";

                // Apply the filter
                query = query.Filter(FilterExpression.Parse(filterString));

            }

            return query;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            // Inbox is always filtered per user
            IEnumerable<AbstractPermission> permissions = new List<AbstractPermission> {
                new AbstractPermission
                {
                     View = "-", // Not important
                     Action = "Read"
                }
            };

            return Task.FromResult(permissions);
        }

        protected override async Task<Dictionary<string, object>> GetExtras(IEnumerable<InboxRecord> result)
        {
            var userInfo = await _repo.GetUserInfoAsync();
            var userIdSingleton = new List<int> { userInfo.UserId.Value };
            var info = (await _repo.InboxCounts__Load(userIdSingleton)).FirstOrDefault();

            var extras = new Dictionary<string, object>
            {
                ["Count"] = info?.Count,
                ["UnknownCount"] = info?.UnknownCount
            };

            return extras;
        }
    }
}
