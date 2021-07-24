using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class OutboxService : FactWithIdServiceBase<OutboxRecord, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;

        public OutboxService(ApplicationFactServiceBehavior behavior, FactServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
        }

        protected override string View => throw new NotImplementedException(); // We override UserPermissions

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken _)
        {
            var result = ExpressionOrderBy.Parse("CreatedAt desc");
            return Task.FromResult(result);
        }

        protected override Task<EntityQuery<OutboxRecord>> Search(EntityQuery<OutboxRecord> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var assigneeProp = nameof(OutboxRecord.Assignee);
                var nameProp = $"{assigneeProp}.{nameof(User.Name)}";
                var name2Prop = $"{assigneeProp}.{nameof(User.Name2)}";
                var name3Prop = $"{assigneeProp}.{nameof(User.Name3)}";

                var commentProp = nameof(OutboxRecord.Comment);
                var memoProp = $"{nameof(OutboxRecord.Document)}.{nameof(Document.Memo)}";

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
            return new FilteredQueryFactory<InboxRecord>(factory, "AssigneeId eq me"); // Every user sees just their own inbox
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
    }
}
