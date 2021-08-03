using Tellma.Model.Common;

namespace Tellma.Repository.Common
{
    public interface IQueryFactory
    {
        EntityQuery<T> EntityQuery<T>() where T : Entity;

        FactQuery<T> FactQuery<T>() where T : Entity;

        AggregateQuery<T> AggregateQuery<T>() where T : Entity;
    }
}
