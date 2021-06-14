using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Jobs;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class InboxController : FactWithIdControllerBase<InboxRecord, int>
    {
        public const string BASE_ADDRESS = "inbox";

        private readonly InboxService _service;

        public InboxController(InboxService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpPut("check")]
        public async Task<ActionResult> CheckInbox([FromBody] DateTimeOffset now)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                await _service.CheckInbox(now);
                return Ok();
            }
            , _logger);
        }

        protected override FactWithIdServiceBase<InboxRecord, int> GetFactWithIdService()
        {
            return _service;
        }
    }

    public class InboxService : FactWithIdServiceBase<InboxRecord, int>
    {
        private readonly ApplicationRepository _repo;
        private readonly InboxNotificationsService _inboxService;

        public InboxService(
            ApplicationRepository repo,
            InboxNotificationsService inboxService,
            IServiceProvider sp) : base(sp)
        {
            _repo = repo;
            _inboxService = inboxService;
        }

        public async Task CheckInbox(DateTimeOffset now)
        {
            var infos = await _repo.Inbox__Check(now);

            // Notify the user
            var tenantId = _tenantIdAccessor.GetTenantId();
            _inboxService.NotifyInbox(tenantId, infos, updateInboxList: false);
        }

        protected override ExpressionOrderBy DefaultOrderBy()
        {
            return ExpressionOrderBy.Parse("CreatedAt desc");
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<InboxRecord> Search(Query<InboxRecord> query, GetArguments args)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var createdByProp = nameof(InboxRecord.CreatedBy);
                var nameProp = $"{createdByProp}.{nameof(User.Name)}";
                var name2Prop = $"{createdByProp}.{nameof(User.Name2)}";
                var name3Prop = $"{createdByProp}.{nameof(User.Name3)}";

                var commentProp = nameof(InboxRecord.Comment);
                var memoProp = $"{nameof(InboxRecord.Document)}.{nameof(Document.Memo)}";

                // Prepare the filter string
                var filterString = $"{nameProp} contains '{search}' or {name2Prop} contains '{search}' or {name3Prop} contains '{search}' or {commentProp} contains '{search}' or {memoProp} contains '{search}'";

                // Apply the filter
                query = query.Filter(ExpressionFilter.Parse(filterString));

            }

            return query;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
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

        protected override async Task<Extras> GetExtras(IEnumerable<InboxRecord> result, CancellationToken cancellation)
        {
            var userInfo = await _repo.GetUserInfoAsync(cancellation);
            var userIdSingleton = new List<int> { userInfo.UserId.Value };
            var info = (await _repo.InboxCounts__Load(userIdSingleton, cancellation)).FirstOrDefault();

            var extras = new Extras
            {
                ["Count"] = info?.Count,
                ["UnknownCount"] = info?.UnknownCount
            };

            return extras;
        }
    }
}
