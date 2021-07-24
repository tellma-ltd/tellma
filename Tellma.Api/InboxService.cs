using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class InboxService : FactWithIdServiceBase<InboxRecord, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IClientProxy _client;

        public InboxService(ApplicationFactServiceBehavior behavior, FactServiceDependencies deps, IClientProxy client) : base(deps)
        {
            _behavior = behavior;
            _client = client;
        }

        protected override string View => throw new NotImplementedException(); // We override UserPermissions

        protected override IFactServiceBehavior FactBehavior => _behavior; 

        public async Task CheckInbox(DateTimeOffset now)
        {
            await Initialize();

            var infos = await _behavior.Repository.Inbox__Check(now, userId: UserId);

            // Notify the user
            _client.UpdateInboxStatuses(_behavior.TenantId, infos, updateInboxList: false);
        }

        protected override Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken cancellation)
        {
            var result = ExpressionOrderBy.Parse("CreatedAt desc");
            return Task.FromResult(result);
        }

        protected override Task<EntityQuery<InboxRecord>> Search(EntityQuery<InboxRecord> query, GetArguments args, CancellationToken _)
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

            return Task.FromResult(query);
        }

        protected override IQueryFactory QueryFactory()
        {
            var factory = base.QueryFactory();
            return new FilteredQueryFactory<OutboxRecord>(factory, "CreatedById eq me"); // Every user sees just their own inbox
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            static IEnumerable<AbstractPermission> ReadAll()
            {
                yield return new AbstractPermission { Action = "Read" };
            }

            // User always has permission to view their entire inbox
            return Task.FromResult(ReadAll());
        }

        protected override async Task<Extras> GetExtras(IEnumerable<InboxRecord> result, CancellationToken cancellation)
        {
            var userIdSingleton = new List<int> { UserId };
            var statusSingleton = await _behavior.Repository.InboxCounts__Load(userIdSingleton, cancellation);
            var status = statusSingleton.FirstOrDefault();

            var extras = new Extras
            {
                ["Count"] = status?.Count,
                ["UnknownCount"] = status?.UnknownCount
            };

            return extras;
        }
    }
}
