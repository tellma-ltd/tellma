using BSharp.Data.Queries;
using BSharp.EntityModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BSharp.Data
{
    public interface IRepository
    {
        Query<T> Query<T>() where T : Entity;

        AggregateQuery<T> AggregateQuery<T>() where T : Entity;

        // Task<Query<T>> AsQueryAsync<T, TForSave>(List<TForSave> entities) where T : Entity where TForSave : Entity;
    }
}
