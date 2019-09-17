using BSharp.Data.Queries;
using BSharp.Entities;

namespace BSharp.Data
{
    public interface IRepository
    {
        Query<T> Query<T>() where T : Entity;

        AggregateQuery<T> AggregateQuery<T>() where T : Entity;
    }
}
