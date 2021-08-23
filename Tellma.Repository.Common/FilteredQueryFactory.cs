using Tellma.Model.Common;

namespace Tellma.Repository.Common
{
    public class FilteredQueryFactory<TFiltered> : IQueryFactory where TFiltered : Entity
    {
        private readonly IQueryFactory _baseFactory;
        private readonly ExpressionFilter _filter;

        public FilteredQueryFactory(IQueryFactory baseFactory, string filter)
        {
            _baseFactory = baseFactory;
            _filter = ExpressionFilter.Parse(filter);
        }

        public AggregateQuery<T> AggregateQuery<T>() where T : Entity
        {
            if (typeof(T) == typeof(TFiltered))
            {
                return _baseFactory.AggregateQuery<T>().Filter(_filter);
            }
            else
            {
                return _baseFactory.AggregateQuery<T>();
            }
        }

        public EntityQuery<T> EntityQuery<T>() where T : Entity
        {
            if (typeof(T) == typeof(TFiltered))
            {
                return _baseFactory.EntityQuery<T>().Filter(_filter);
            }
            else
            {
                return _baseFactory.EntityQuery<T>();
            }
        }

        public FactQuery<T> FactQuery<T>() where T : Entity
        {
            if (typeof(T) == typeof(TFiltered))
            {
                return _baseFactory.FactQuery<T>().Filter(_filter);
            }
            else
            {
                return _baseFactory.FactQuery<T>();
            }
        }
    }
}
