using BSharp.Data;
using BSharp.Data.Queries;
using BSharp.Entities;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Controllers.Utilities
{
    /// <summary>
    /// A wrapper around another <see cref="IRepository"/> which always adds a collection of parameters
    /// to the query with the type <see cref="TParametered"/>. This is convenient in controllers
    /// that give access to a parametered fact table
    /// </summary>
    /// <typeparam name="TParametered">The type whose fact table requires parameters</typeparam>
    public class ParameteredRepository<TParametered> : IRepository where TParametered : Entity
    {
        private readonly IRepository _repo;
        private readonly (string ParamName, object Value)[] _additionalParameters;

        public ParameteredRepository(IRepository repo, params (string ParamName, object Value)[] additionalParameters)
        {
            _repo = repo;
            _additionalParameters = additionalParameters.ToArray();
        }

        public AggregateQuery<T> AggregateQuery<T>() where T : Entity
        {
            if (typeof(T) == typeof(TParametered))
            {
                return _repo.AggregateQuery<T>().AdditionalParameters(_additionalParameters);
            }
            else
            {
                return _repo.AggregateQuery<T>();
            }
        }

        public Query<T> Query<T>() where T : Entity
        {
            if(typeof(T) == typeof(TParametered))
            {
                return _repo.Query<T>().AdditionalParameters(_additionalParameters);
            }
            else
            {
                return _repo.Query<T>();
            }
        }
    }
}
