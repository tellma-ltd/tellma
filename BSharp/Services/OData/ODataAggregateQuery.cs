using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Services.Utilities;
using Microsoft.Extensions.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BSharp.Services.OData
{
    public class ODataAggregateQuery<T> where T : DtoBase
    {
        private readonly SqlConnection _conn;
        private readonly Func<Type, string> _sources;
        private readonly IStringLocalizer _localizer;
        private readonly int _userId;
        private readonly TimeZoneInfo _userTimeZone;

        private int? _top;
        private List<string> _filterConditions;
        private string _select;
        private SqlParameter[] _parameters;
        private SqlTransaction _trx;

        public ODataAggregateQuery(DbConnection conn, Func<Type, string> sources, IStringLocalizer localizer, int userId, TimeZoneInfo userTimeZone)
        {
            if (!(conn is SqlConnection))
            {
                throw new InvalidOperationException("Only Microsoft SQL Server is supported");
            }

            _conn = conn as SqlConnection;
            _sources = sources;
            _localizer = localizer;
            _userId = userId;
            _userTimeZone = userTimeZone;
        }

        public ODataAggregateQuery<T> Clone()
        {
            var clone = new ODataAggregateQuery<T>(_conn, _sources, _localizer, _userId, _userTimeZone)
            {
                _top = _top,
                _filterConditions = _filterConditions?.ToList(),
                _select = _select,
                _parameters = _parameters?.ToArray(),
                _trx = _trx
            };

            return clone;
        }

        public ODataAggregateQuery<T> Select(string paths)
        {
            if (string.IsNullOrWhiteSpace(paths))
            {
                paths = null;
            }

            _select = paths;
            return this;
        }

        public ODataAggregateQuery<T> Filter(string condition)
        {
            if (!string.IsNullOrWhiteSpace(condition))
            {
                _filterConditions = _filterConditions ?? new List<string>();
                _filterConditions.Add(condition);
            }

            if (_top != null)
            {
                throw new InvalidOperationException("Cannot filter the query again after either Skip or Top have been invoked");
            }

            return this;
        }

        public ODataAggregateQuery<T> Top(int top)
        {
            _top = top;
            return this;
        }

        public ODataAggregateQuery<T> UseTransaction(DbTransaction trx)
        {
            if (!(trx is SqlTransaction))
            {
                throw new InvalidOperationException("Only Microsoft SQL Server is supported");
            }

            _trx = trx as SqlTransaction;

            return this;
        }

        public async Task<List<DtoBase>> ToListAsync()
        {
            // ------------------------ Validation Step
            // Create the expressions. As for filter: turn all the filters into expressions and AND them together
            var selectExp = SelectAggregateExpression.Parse(_select);
            FilterExpression filterExp = _filterConditions?.Select(c => FilterExpression.Parse(c))
                .Aggregate((e1, e2) => new FilterConjunction { Left = e1, Right = e2 });

            // Step 1 - Validate the arguments
            ValidatePathsAndProperties(selectExp, filterExp);
            if (selectExp == null)
            {
                string message = $"The select argument is required";
                throw new BadRequestException(message);
            }

            //// ------------------------ DTOable analysis
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


            IQueryInternal query = new ODataAggregateQueryInternal
            {
                ResultType = typeof(T),
                Select = selectExp,
                Filter = filterExp,
                Top = _top
            };

            var ps = new SqlStatementParameters();
            SqlStatement statement = query.PrepareStatement(_sources, ps, _userId, _userTimeZone);

            var queries = new List<(IQueryInternal Query, SqlStatement Statement)> { (query, statement) };
            var result = await ObjectLoader.LoadStatements<T>(
                queries: queries, 
                preparatorySql: null, 
                ps: ps, 
                conn: _conn, 
                trx:_trx);

            return result;
        }

        /// <summary>
        /// To prevent SQL injection attacks
        /// </summary>
        private void ValidatePathsAndProperties(SelectAggregateExpression selectExp, FilterExpression filterExp)
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
                selectPathValidator.Validate(typeof(T), _localizer, "select",
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
                filterPathTree.Validate(typeof(T), _localizer, "filter",
                    allowLists: false,
                    allowSimpleTerminals: true,
                    allowNavigationTerminals: false);
            }
        }
    }
}
