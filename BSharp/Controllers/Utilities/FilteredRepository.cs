using BSharp.Data;
using BSharp.Data.Queries;
using BSharp.Entities;

namespace BSharp.Controllers.Utilities
{
    /// <summary>
    /// A wrapper around another <see cref="IRepository"/> which always adds a specified filter
    /// to the query with the type <see cref="TFiltered"/>. This is convenient in controllers
    /// that have a definitionId in their context
    /// </summary>
    /// <typeparam name="TFiltered">The type of filter to apply the query to</typeparam>
    public class FilteredRepository<TFiltered> : IRepository where TFiltered : Entity
    {
        private readonly IRepository _repo;
        private readonly FilterExpression _filter;

        public FilteredRepository(IRepository repo, string filter)
        {
            _repo = repo;
            _filter = FilterExpression.Parse(filter);
        }

        public AggregateQuery<T> AggregateQuery<T>() where T : Entity
        {
            if (typeof(T) == typeof(TFiltered))
            {
                return _repo.AggregateQuery<T>().Filter(_filter);
            }
            else
            {
                return _repo.AggregateQuery<T>();
            }
        }

        public Query<T> Query<T>() where T : Entity
        {
            if(typeof(T) == typeof(TFiltered))
            {
                return _repo.Query<T>().Filter(_filter);
            }
            else
            {
                return _repo.Query<T>();
            }
        }
    }
}
