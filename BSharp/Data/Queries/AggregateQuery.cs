using BSharp.Controllers.Misc;
using BSharp.EntityModel;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data.Queries
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
        private List<FilterExpression> _filterConditions;
        private AggregateSelectExpression _select;
        private SqlParameter[] _parameters;

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
                _parameters = _parameters?.ToArray(),
            };

            return clone;
        }

        /// <summary>
        /// Applies a <see cref="AggregateSelectExpression"/> to specify which dimensions and measures
        /// must be returned, dimensions are specified without an aggregate function, measures do not have an aggregate function
        /// </summary>
        public AggregateQuery<T> Select(AggregateSelectExpression selects)
        {
            var clone = this.Clone();
            clone._select = selects;
            return clone;
        }

        /// <summary>
        /// Applies a <see cref="FilterExpression"/> to filter the result
        /// </summary>
        public AggregateQuery<T> Filter(FilterExpression condition)
        {
            if (_top != null)
            {
                // Programmer mistake
                throw new InvalidOperationException($"Cannot filter the query after {nameof(Top)} has been invoked");
            }

            var clone = Clone();
            if (condition != null)
            {
                clone._filterConditions.Add(condition);
            }

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


        public async Task<List<Entity>> ToListAsync()
        {
            var args = await _factory();
            var conn = args.Connection;
            var sources = args.Sources;
            var userId = args.UserId;
            var userTimeZone = args.UserTimeZone;
            var localizer = args.Localizer;

            // ------------------------ Validation Step
            // Create the expressions. As for filter: turn all the filters into expressions and AND them together
            AggregateSelectExpression selectExp = _select;
            FilterExpression filterExp = _filterConditions?.Aggregate(
                (e1, e2) => new FilterConjunction { Left = e1, Right = e2 });

            // Step 1 - Validate the arguments
            if (selectExp == null)
            {
                string message = $"The select argument is required";
                throw new InvalidOperationException(message);
            }

            // To prevent SQL injection
            ValidatePathsAndProperties(selectExp, filterExp, localizer);

            //// ------------------------ Entityable analysis
            //// Grab all paths that terminate with "Id"
            //var idAtoms = selectExp.Where(e => e.Property == "Id");
            //var dtoableAtoms = new List<SelectAggregateAtom>();

            //// Any atom in the select that begins with an id path, add it to dtoablePaths
            //foreach (var idPath in idAtoms.Select(e => e.Path))
            //{
            //    foreach (var selectAtom in selectExp)
            //    {
            //        if (idPath.Length <= selectAtom.Path.Length)
            //        {
            //            bool match = true;
            //            for (int i = 0; i < idPath.Length; i++)
            //            {
            //                if (idPath[i] != selectAtom.Path[i])
            //                {
            //                    match = false;
            //                    break;
            //                }
            //            }

            //            if (match)
            //            {
            //                selectAtom.Aggregation = null; // A DTOable atom cannot have an aggregation
            //                dtoableAtoms.Add(selectAtom);
            //            }
            //        }
            //    }
            //}

            //// This now contains all paths that are DTOable
            //dtoableAtoms.AddRange(idAtoms);

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
            IQueryInternal query = new AggregateQueryInternal
            {
                ResultType = typeof(T),
                Select = selectExp,
                Filter = filterExp,
                Top = _top
            };

            // Prepare the statement from the internal query
            var ps = new SqlStatementParameters();
            var rawSources = QueryTools.RawSources(sources, ps);
            SqlStatement statement = query.PrepareStatement(rawSources, ps, userId, userTimeZone);

            // load the entities and return them
            var queries = new List<(IQueryInternal Query, SqlStatement Statement)> { (query, statement) };
            var result = await EntityLoader.LoadStatements<T>(
                queries: queries,
                preparatorySql: null,
                ps: ps,
                conn: conn);

            return result;
        }

        /// <summary>
        /// Protects against SQL injection attacks
        /// </summary>
        private void ValidatePathsAndProperties(AggregateSelectExpression selectExp, FilterExpression filterExp, IStringLocalizer localizer)
        {
            // This is important to avoid SQL injection attacks

            // Select
            if (selectExp != null)
            {
                PathValidator selectPathValidator = new PathValidator();
                foreach (var atom in selectExp)
                {
                    // AddPath(atom.Path, atom.Property);
                    selectPathValidator.AddPath(atom.Path, atom.Property);
                }

                // Make sure the paths are valid (Protects against SQL injection)
                selectPathValidator.Validate(typeof(T), localizer, "select",
                    allowLists: false,
                    allowSimpleTerminals: true,
                    allowNavigationTerminals: false);
            }

            // Filter
            if (filterExp != null)
            {
                PathValidator filterPathTree = new PathValidator();
                foreach (var atom in filterExp)
                {
                    // AddPath(atom.Path, atom.Property);
                    filterPathTree.AddPath(atom.Path, atom.Property);
                }

                // Make sure the paths are valid (Protects against SQL injection)
                filterPathTree.Validate(typeof(T), localizer, "filter",
                    allowLists: false,
                    allowSimpleTerminals: true,
                    allowNavigationTerminals: false);
            }
        }
    }
}
