using Tellma.Entities;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Tellma.Entities.Descriptors;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Used to execute GROUP BY queries on a SQL Server database
    /// </summary>
    /// <typeparam name="T">The expected type of the result</typeparam>
    public class AggregateQuery<T> where T : Entity
    {
        // From constructor
        private readonly QueryArgumentsFactory _factory;

        // Through setter methods
        private int? _top;
        private List<ExpressionFilter> _filterConditions;
        private ExpressionHaving _having;
        private ExpressionAggregateSelect _select;
        private ExpressionAggregateOrderBy _orderby;
        private List<SqlParameter> _additionalParameters;

        /// <summary>
        /// Creates an instance of <see cref="AggregateQuery{T}"/>
        /// </summary>
        /// <param name="conn">The connection to use when loading the results</param>
        /// <param name="sources">Mapping from every type into SQL code that can be used to query that type</param>
        /// <param name="localizer">For validation error messages</param>
        /// <param name="userId">Used as context when preparing certain filter expressions</param>
        /// <param name="userTimeZone">Used as context when preparing certain filter expressions</param>
        public AggregateQuery(QueryArgumentsFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Clones the <see cref="AggregateQuery{T}"/> into a new one. Used internally
        /// </summary>
        private AggregateQuery<T> Clone()
        {
            var clone = new AggregateQuery<T>(_factory)
            {
                _top = _top,
                _filterConditions = _filterConditions?.ToList(),
                _select = _select,
                _additionalParameters = _additionalParameters?.ToList()
            };

            return clone;
        }

        /// <summary>
        /// Applies a <see cref="ExpressionAggregateSelect"/> to specify which dimensions and measures
        /// must be returned, dimensions are specified without an aggregate function, measures do not have an aggregate function
        /// </summary>
        public AggregateQuery<T> Select(ExpressionAggregateSelect selects)
        {
            var clone = Clone();
            clone._select = selects;
            return clone;
        }

        /// <summary>
        /// Applies a <see cref="ExpressionAggregateOrderBy"/> to specify which dimensions and measures
        /// must be returned, dimensions are specified without an aggregate function, measures do not have an aggregate function
        /// </summary>
        public AggregateQuery<T> OrderBy(ExpressionAggregateOrderBy orderby)
        {
            var clone = Clone();
            clone._orderby = orderby;
            return clone;
        }

        /// <summary>
        /// Applies a <see cref="ExpressionFilter"/> to filter the result
        /// </summary>
        public AggregateQuery<T> Filter(ExpressionFilter condition)
        {
            if (_top != null)
            {
                // Programmer mistake
                throw new InvalidOperationException($"Cannot filter the query after {nameof(Top)} has been invoked");
            }

            var clone = Clone();
            if (condition != null)
            {
                clone._filterConditions ??= new List<ExpressionFilter>();
                clone._filterConditions.Add(condition);
            }

            return clone;
        }

        /// <summary>
        /// Applies a <see cref="ExpressionHaving"/> to filter the grouped result
        /// </summary>
        public AggregateQuery<T> Having(ExpressionHaving having)
        {
            if (_top != null)
            {
                // Programmer mistake
                throw new InvalidOperationException($"Cannot apply a having argument after {nameof(Top)} has been invoked");
            }

            var clone = Clone();
            clone._having = having;
            return clone;
        }

        /// <summary>
        /// Instructs the query to load only the top N rows
        /// </summary>
        public AggregateQuery<T> Top(int top)
        {
            var clone = Clone();
            clone._top = top;
            return clone;
        }

        /// <summary>
        /// If the Query is for a parametered fact table such as <see cref="SummaryEntry"/>, the parameters
        /// must be supplied this method must be supplied through this method before loading any data
        /// </summary>
        public AggregateQuery<T> AdditionalParameters(params SqlParameter[] parameters)
        {
            var clone = Clone();
            if (clone._additionalParameters == null)
            {
                clone._additionalParameters = new List<SqlParameter>();
            }

            clone._additionalParameters.AddRange(parameters);

            return clone;
        }

        public async Task<List<DynamicRow>> ToListAsync(CancellationToken cancellation)
        {
            var args = await _factory(cancellation);

            var conn = args.Connection;
            var sources = args.Sources;
            var userId = args.UserId;
            var userToday = args.UserToday;
            var localizer = args.Localizer;

            var rootDesc = TypeDescriptor.Get<T>();

            // ------------------------ Validation Step

            // SELECT Validation
            ExpressionAggregateSelect selectExp = _select;
            if (selectExp == null)
            {
                string message = $"The select argument is required";
                throw new InvalidOperationException(message);
            }

            // Make sure that measures are well formed: every column access is wrapped inside an aggregation function
            foreach (var exp in selectExp)
            {
                if (exp.ContainsAggregations) // This is a measure
                {
                    // Every column access must descend from an aggregation function
                    var exposedColumnAccess = exp.UnaggregatedColumnAccesses().FirstOrDefault();
                    if (exposedColumnAccess != null)
                    {
                        throw new QueryException($"Select parameter contains a measure with a column access {exposedColumnAccess} that is not included within an aggregation.");
                    }
                }
            }

            // ORDER BY Validation
            ExpressionAggregateOrderBy orderbyExp = _orderby;
            if (orderbyExp != null)
            {
                var selectedDims = selectExp.Where(e => !e.ContainsAggregations); // Every dimension in orderby must also be present in select

                foreach (var exp in orderbyExp)
                {
                    // Order by cannot be a constant
                    if (!exp.ContainsAggregations && !exp.ContainsColumnAccesses)
                    {
                        throw new QueryException("OrderBy parameter cannot be a constant, every order by expression must contain either an aggregation or a column access.");
                    }

                    // If it's a dimension, it must be mentioned in the select
                    if (!exp.ContainsAggregations)
                    {
                        // TODO
                    }
                }
            }

            // FILTER Validation
            ExpressionFilter filterExp = null;
            if (_filterConditions != null)
            {
                var conditionWithAggregation = _filterConditions.FirstOrDefault(e => e.Expression.ContainsAggregations);
                if (conditionWithAggregation != null)
                {
                    throw new QueryException($"Filter contains a condition with an aggregation function: {conditionWithAggregation}");
                }

                filterExp = _filterConditions.Aggregate(
                    (e1, e2) => ExpressionFilter.Conjunction(e1, e2)); // AND the conditions together
            }


            // HAVING Validation
            ExpressionHaving havingExp = _having;
            if (havingExp != null)
            {
                // Every column access must descend from an aggregation function
                var exposedColumnAccess = havingExp.Expression.UnaggregatedColumnAccesses().FirstOrDefault();
                if (exposedColumnAccess != null)
                {
                    throw new QueryException($"Having parameter contains a column access {exposedColumnAccess} that is not included within an aggregation.");
                }

            }

            //// ------------------------ Tree analysis
            //// Grab all paths that contain a Parent property, and 
            //var trees = new List<(Type TreeType, ArraySegment<string> PathToTreeEntity, ArraySegment<string> PathFromTreeEntity, string Property)>();
            //var treeAtoms = new HashSet<SelectAggregateAtom>();
            //foreach (var atom in dtoableAtoms)
            //{
            //    var currentType = typeof(T);
            //    for (var i = 0; i < atom.Path.Length; i++)
            //    {
            //        var step = atom.Path[i];
            //        var pathProp = currentType.GetProperty(step);
            //        if (pathProp.IsParent())
            //        {
            //            var treeType = currentType;
            //            var pathToTreeEntity = new ArraySegment<string>(atom.Path, 0, i);
            //            var pathFromTreeEntity = new ArraySegment<string>(atom.Path, i + 1, atom.Path.Length - (i + 1));
            //            var property = atom.Property;

            //            trees.Add((treeType, pathToTreeEntity, pathFromTreeEntity, property));
            //            treeAtoms.Add(atom);
            //        }

            //        currentType = pathProp.PropertyType;
            //    }

            //    var prop = currentType.GetProperty(atom.Property);
            //}

            //// Keep only the paths that are not a DTOable trees, those will be loaded separately
            //selectExp = new SelectAggregateExpression(selectExp.Where(e => treeAtoms.Contains(e)));


            // Prepare the internal query (this one should not have any select paths containing Parent property)
            AggregateQueryInternal query = new AggregateQueryInternal
            {
                ResultType = rootDesc,
                Select = selectExp,
                Filter = filterExp,
                OrderBy = orderbyExp,
                Having = havingExp,
                Top = _top
            };

            // Prepare the variables and parameters
            var vars = new SqlStatementVariables();
            var ps = new SqlStatementParameters();

            if (_additionalParameters != null)
            {
                foreach (var additionalParameter in _additionalParameters)
                {
                    ps.AddParameter(additionalParameter);
                }
            }

            SqlStatement statement = query.PrepareStatement(sources, vars, ps, userId, userToday);

            // load the entities and return them
            var result = await EntityLoader.LoadAggregateStatement(
                statement: statement,
                vars: vars,
                ps: ps,
                conn: conn,
                cancellation: cancellation);

            return result;
        }

        ///// <summary>
        ///// Protects against SQL injection attacks
        ///// </summary>
        //private void ValidatePathsAndProperties(ExpressionAggregateSelect selectExp, ExpressionFilter filterExp, TypeDescriptor rootDesc, IStringLocalizer localizer)
        //{
        //    // This is important to avoid SQL injection attacks

        //    // Select
        //    if (selectExp != null)
        //    {
        //        PathValidator selectPathValidator = new PathValidator();
        //        foreach (var atom in selectExp)
        //        {
        //            // AddPath(atom.Path, atom.Property);
        //            selectPathValidator.AddPath(atom.Path, atom.Property);
        //        }

        //        // Make sure the paths are valid (Protects against SQL injection)
        //        selectPathValidator.Validate(rootDesc, localizer, "select",
        //            allowLists: false,
        //            allowSimpleTerminals: true,
        //            allowNavigationTerminals: false);
        //    }

        //    // Filter
        //    if (filterExp != null)
        //    {
        //        PathValidator filterPathTree = new PathValidator();
        //        foreach (var atom in filterExp)
        //        {
        //            // AddPath(atom.Path, atom.Property);
        //            filterPathTree.AddPath(atom.Path, atom.Property);
        //        }

        //        // Make sure the paths are valid (Protects against SQL injection)
        //        filterPathTree.Validate(rootDesc, localizer, "filter",
        //            allowLists: false,
        //            allowSimpleTerminals: true,
        //            allowNavigationTerminals: false);
        //    }
        //}
    }
}
