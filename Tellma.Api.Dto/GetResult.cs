using System.Collections.Generic;
using Tellma.Model.Common;

namespace Tellma.Api.Dto
{
    public class GetResult<TEntity> where TEntity : Entity
    {
        public GetResult(IEnumerable<TEntity> data, int? count)
        {
            Data = data;
            Count = count;
        }

        public IEnumerable<TEntity> Data { get; }
        public int? Count { get; }
    }
}
