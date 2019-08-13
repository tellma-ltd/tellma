using BSharp.Data.Queries;
using BSharp.EntityModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BSharp.Data
{
    public interface IRepository
    {
        Task<Query<T>> QueryAsync<T>() where T : Entity;

        Task<AggregateQuery<T>> AggregateQueryAsync<T>() where T : Entity;
    }
}
