using Tellma.Data.Queries;
using Tellma.Model.Application;

namespace Tellma.Data
{
    public interface IRepository
    {
        Query<T> Query<T>() where T : Entity;

        FactQuery<T> FactQuery<T>() where T : Entity;

        AggregateQuery<T> AggregateQuery<T>() where T : Entity;
    }
}
