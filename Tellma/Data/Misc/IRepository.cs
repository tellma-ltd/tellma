using Tellma.Data.Queries;
using Tellma.Entities;

namespace Tellma.Data
{
    public interface IRepository
    {
        Query<T> Query<T>() where T : Entity;

        AggregateQuery<T> AggregateQuery<T>() where T : Entity;
    }
}
