using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Templating;
using Tellma.Model.Common;

namespace Tellma.Api
{
    public class ApiLowLevelClient : IApiClientForTemplating
    {
        public Task<IReadOnlyList<DynamicRow>> GetAggregate(string collection, int? definitionId, string select, string filter, string having, string orderby, int? top, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Entity>> GetEntities(string collection, int? definitionId, string select, string filter, string orderby, int? top, int? skip, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<EntityWithKey>> GetEntitiesByIds(string collection, int? definitionId, string select, IList ids, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        public Task<EntityWithKey> GetEntityById(string collection, int? definitionId, string select, object id, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<DynamicRow>> GetFact(string collection, int? definitionId, string select, string filter, string orderby, int? top, int? skip, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }
    }
}
