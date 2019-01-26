using AutoMapper;
using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Services.Identity;
using BSharp.Services.ImportExport;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using M = BSharp.Data.Model;

namespace BSharp.Controllers
{
    [ApiController]
    public abstract class ReadControllerBase<TModel, TDto, TKey> : ControllerBase
        where TModel : M.ModelBase
        where TDto : DtoKeyBase<TKey>
    {
        // Constants

        private const int DEFAULT_MAX_PAGE_SIZE = 10000;

        // Private Fields

        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        protected static ConcurrentDictionary<Type, string> _getCollectionNameCache = new ConcurrentDictionary<Type, string>(); // This cache never expires

        // Constructor

        public ReadControllerBase(ILogger logger, IStringLocalizer localizer, IMapper mapper, IUserService userService)
        {
            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
            _userService = userService;
        }

        // HTTP Methods

        [HttpGet]
        public virtual async Task<ActionResult<GetResponse<TDto>>> Get([FromQuery] GetArguments args)
        {
            try
            {
                var result = await GetImplAsync(args);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<GetByIdResponse<TDto>>> GetById(TKey id, [FromQuery] GetByIdArguments args)
        {
            try
            {
                // TODO Authorize GET by Id

                // Expand
                var query = SingletonQuery(GetBaseQuery(), id);
                query = Expand(query, args.Expand);

                // Load
                var dbEntity = await query.FirstOrDefaultAsync();
                if (dbEntity == null)
                {
                    throw new NotFoundException<TKey>(id);
                }

                // Flatten Related Entities
                var relatedEntities = FlattenRelatedEntities(dbEntity, args.Expand);

                // Map the primary result to DTO too
                var entity = Map(dbEntity);

                // Return
                var result = new GetByIdResponse<TDto>
                {
                    Entity = entity,
                    CollectionName = GetCollectionName(typeof(TDto)),
                    RelatedEntities = relatedEntities
                };

                return Ok(result);
            }
            catch (NotFoundException<TKey> ex)
            {
                return NotFound(ex.Ids);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("export")]
        public virtual async Task<ActionResult> Export([FromQuery] ExportArguments args)
        {
            try
            {
                // Get abstract grid
                var response = await GetImplAsync(args);
                var abstractFile = DtosToAbstractGrid(response, args);
                return AbstractGridToFileResult(abstractFile, args.Format);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }


        // Abstract and virtual members

        /// <summary>
        /// Returns the query from which the GET endpoint retrieves the results
        /// </summary>
        protected abstract IQueryable<TModel> GetBaseQuery();

        /// <summary>
        /// Returns the query from which the GET by Id endpoint retrieves the result
        /// </summary>
        protected abstract IQueryable<TModel> SingletonQuery(IQueryable<TModel> query, TKey id);

        /// <summary>
        /// Applies the search argument, which is handled differently in every controller
        /// </summary>
        protected abstract IQueryable<TModel> Search(IQueryable<TModel> query, string search);

        /// <summary>
        /// Includes or excludes inactive items from the query depending on the boolean switch supplied
        /// </summary>
        protected abstract IQueryable<TModel> IncludeInactive(IQueryable<TModel> query, bool inactive);

        /// <summary>
        /// Transforms a DTO response into an abstract grid that can be transformed into an file
        /// </summary>
        protected abstract AbstractDataGrid DtosToAbstractGrid(GetResponse<TDto> response, ExportArguments args);

        /// <summary>
        /// Returns the entities as per the specifications in the get request
        /// </summary>
        protected virtual async Task<GetResponse<TDto>> GetImplAsync(GetArguments args)
        {
            // TODO Authorize for GET

            // Get a readonly query
            IQueryable<TModel> query = GetBaseQuery().AsNoTracking();

            // Include inactive
            query = IncludeInactive(query, inactive: args.Inactive);

            // Search
            query = Search(query, args.Search);

            // Filter
            query = Filter(query, args.Filter);

            // Before ordering or paging, retrieve the total count
            int totalCount = query.Count();

            // OrderBy
            query = OrderBy(query, args.OrderBy, args.Desc);

            // Apply the paging (Protect against DOS attacks by enforcing a maximum page size)
            var top = args.Top;
            var skip = args.Skip;
            top = Math.Min(top, MaximumPageSize());
            query = query.Skip(skip).Take(top);

            // Apply the expand, which has the general format 'Expand=A,B/C,D'
            query = Expand(query, args.Expand);

            // Load the data, transform it and wrap it in some metadata
            var memoryList = await query.ToListAsync();

            // Flatten related entities and map each to its respective DTO 
            var relatedEntities = FlattenRelatedEntities(memoryList, args.Expand);

            // Map the primary result to DTOs as well
            var resultData = Map(memoryList);

            // Prepare the result in a response object
            var result = new GetResponse<TDto>
            {
                Skip = skip,
                Top = resultData.Count(),
                OrderBy = args.OrderBy,
                Desc = args.Desc,
                TotalCount = totalCount,
                Data = resultData,
                RelatedEntities = relatedEntities,
                CollectionName = GetCollectionName(typeof(TDto))
            };

            // Finally return the result
            return result;
        }

        /// <summary>
        /// Maps a list of the controller models to a list of concrete controller DTOs
        /// </summary>
        protected virtual List<TDto> Map(List<TModel> models)
        {
            return _mapper.Map<List<TDto>>(models);
        }

        /// <summary>
        /// Maps a list of any models to their corresponding DTO types, the default implementation
        /// assumes that the default DTO has been mapped to every model type in AutoMapper by mapping 
        /// it to type <see cref="DtoBase"/>
        /// </summary>
        protected virtual IEnumerable<DtoBase> MapRelatedEntities(IEnumerable<M.ModelBase> relatedEntities)
        {
            return relatedEntities.Select(e => _mapper.Map<DtoBase>(e));
        }

        /// <summary>
        /// Specifies the maximum page size to be returned by GET, defaults to <see cref="DEFAULT_MAX_PAGE_SIZE"/>
        /// </summary>
        protected virtual int MaximumPageSize()
        {
            return DEFAULT_MAX_PAGE_SIZE;
        }

        /// <summary>
        /// Filters the query based on the filter argument, the default implementation 
        /// assumes OData-like syntax
        /// </summary>
        protected virtual IQueryable<TModel> Filter(IQueryable<TModel> query, string filter)
        {
            if (!string.IsNullOrWhiteSpace(filter))
            {
                // Below are the standard steps of any compiler

                //////// (1) Preprocessing

                // Ensure no spaces are repeated
                Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
                filter = regex.Replace(filter, " ");

                // Trim
                filter = filter.Trim();


                //////// (2) Lexical Analysis of string into token stream

                List<string> symbols = new List<string>(new string[] {

                    // Logical Operators
                    " and ", " or ",

                    // Brackets
                    "(", ")",
                });

                List<string> tokens = new List<string>();
                bool insideQuotes = false;
                string acc = "";
                int index = 0;
                while (index < filter.Length)
                {
                    // Lexical anaysis ignores what's inside single quotes
                    if (filter[index] == '\'' && (index == 0 || filter[index - 1] != '\'') && (index == filter.Length - 1 || filter[index + 1] != '\''))
                    {
                        insideQuotes = !insideQuotes;
                        acc += filter[index];
                        index++;
                    }
                    else if (insideQuotes)
                    {
                        acc += filter[index];
                        index++;
                    }
                    else
                    {
                        // Everything that is not inside single quotes is ripe for lexical analysis      
                        var matchingSymbol = symbols.FirstOrDefault(filter.Substring(index).StartsWith);
                        if (matchingSymbol != null)
                        {
                            // Add all that has been accumulating before the symbol
                            if (!string.IsNullOrWhiteSpace(acc))
                            {
                                tokens.Add(acc.Trim());
                                acc = "";
                            }

                            // And add the symbol
                            tokens.Add(matchingSymbol.Trim());
                            index = index + matchingSymbol.Length;
                        }
                        else
                        {
                            acc += filter[index];
                            index++;
                        }
                    }
                }

                if (insideQuotes)
                {
                    // Programmer mistake
                    throw new BadRequestException("Uneven number of single quotation marks in filter query parameter, quotation marks in literals should be escaped by specifying them twice");
                }

                if (!string.IsNullOrWhiteSpace(acc))
                {
                    tokens.Add(acc.Trim());
                }


                //////// (3) Parse token stream to Abstract Syntax Tree (AST)

                Ast ParseToAst(IEnumerable<string> tokenStream)
                {
                    if (tokenStream.IsEnclosedInPairBrackets())
                    {
                        return ParseBrackets(tokenStream);
                    }
                    else if (tokenStream.OutsideBrackets().Any(e => e == "or"))
                    {
                        // OR has lower precedence than AND
                        return ParseDisjunction(tokenStream);
                    }
                    else if (tokenStream.OutsideBrackets().Any(e => e == "and"))
                    {
                        return ParseConjunction(tokenStream);
                    }
                    else if (tokenStream.Count() <= 1)
                    {
                        return ParseAtom(tokenStream);
                    }
                    else
                    {
                        // Programmer mistake
                        throw new BadRequestException("Badly formatted filter parameter");
                    }
                }

                AstBrackets ParseBrackets(IEnumerable<string> tokenStream)
                {
                    return new AstBrackets
                    {
                        Inner = ParseToAst(tokenStream.Skip(1).Take(tokenStream.Count() - 2))
                    };
                }

                AstConjunction ParseConjunction(IEnumerable<string> tokenStream)
                {
                    // find first occurrence of AND outside the brackets, and then parse both sides
                    int i = tokenStream.OutsideBrackets().ToList().IndexOf("and");
                    var left = tokenStream.Take(i);
                    var right = tokenStream.Skip(i + 1);

                    return new AstConjunction
                    {
                        Left = ParseToAst(left),
                        Right = ParseToAst(right),
                    };
                }

                AstDisjunction ParseDisjunction(IEnumerable<string> tokenStream)
                {
                    // find first occurrence of AND outside the brackets, and then parse both sides
                    int i = tokenStream.OutsideBrackets().ToList().IndexOf("or");
                    var left = tokenStream.Take(i);
                    var right = tokenStream.Skip(i + 1);

                    return new AstDisjunction
                    {
                        Left = ParseToAst(left),
                        Right = ParseToAst(right),
                    };
                }

                AstAtom ParseAtom(IEnumerable<string> tokenStream)
                {
                    return new AstAtom { Value = tokenStream.SingleOrDefault() ?? "" };
                }

                Ast ast = ParseToAst(tokens);


                //////// (4) Compile the AST to Linq lambda

                // The parameter on which the expression is based
                var eParam = Expression.Parameter(typeof(TModel));

                // Recursive function to turn the AST to linq
                Expression ToExpression(Ast tree)
                {
                    if (tree is AstBrackets bracketsAst)
                    {
                        return ToExpression(bracketsAst.Inner);
                    }

                    if (tree is AstConjunction conAst)
                    {
                        return Expression.AndAlso(ToExpression(conAst.Left), ToExpression(conAst.Right));
                    }

                    if (tree is AstDisjunction disAst)
                    {
                        return Expression.OrElse(ToExpression(disAst.Left), ToExpression(disAst.Right));
                    }

                    if (tree is AstAtom atom)
                    {
                        var modelType = typeof(TModel);
                        var v = atom.Value;

                        // Indicates a programmer mistake
                        if (string.IsNullOrWhiteSpace(v))
                        {
                            throw new InvalidOperationException("An atomic expression cannot be empty");
                        }
                        // Some controllers may define their own set of keywords which 
                        // take precedence over the default parsing of expression atoms
                        Expression result = ParseSpecialFilterKeyword(v, eParam);

                        // If the controller does not handle this atom, we use the default parser
                        if (result == null)
                        {
                            // The default parser assumes the following syntax: Path Op Value, for example: Address/Street eq 'Huntington Rd.'
                            var pieces = v.Split(" ");
                            if (pieces.Count() < 3)
                            {
                                // Programmer mistake
                                throw new InvalidOperationException("An atomic expression must either be a reserved word or come in the form of 'Property Op Value'");
                            }
                            else
                            {
                                // (A) Parse the member access path (e.g. "Address/Street")
                                var path = pieces[0];

                                var steps = path.Split('/');
                                PropertyInfo prop = null;
                                Type propType = modelType;
                                Expression memberAccess = eParam;
                                foreach (var step in steps)
                                {
                                    prop = propType.GetProperty(step);
                                    if (prop == null)
                                    {
                                        throw new InvalidOperationException(
                                            $"The property '{step}' from the filter argument is not a navigation property of entity type '{propType.Name}'.");
                                    }

                                    var isCollection = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>);
                                    if (isCollection)
                                    {
                                        // Programmer mistake
                                        throw new InvalidOperationException("Filter parameters cannot access collection properties");
                                    }

                                    propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                                    memberAccess = Expression.Property(memberAccess, prop);
                                }

                                // (B) Parse the value (e.g. "'Huntington Rd.'")
                                var valueString = string.Join(" ", pieces.Skip(2));
                                object value;
                                if (valueString == "null")
                                {
                                    value = null;
                                }
                                else
                                {
                                    if (propType == typeof(string) || propType == typeof(char) || propType == typeof(DateTimeOffset) || propType == typeof(DateTime))
                                    {
                                        if (!valueString.StartsWith("'") || !valueString.EndsWith("'"))
                                        {
                                            // Programmer mistake
                                            throw new InvalidOperationException($"Property {prop.Name} is of type String, therefore the value it is compared to must be enclosed in single quotation marks");
                                        }

                                        valueString = valueString.Substring(1, valueString.Length - 2);
                                    }

                                    try
                                    {
                                        // The default Convert.ChangeType cannot handle converting types to nullable types
                                        // Therefore this method overcomes this limitation, credit: https://bit.ly/2DgqJmL
                                        object ChangeType(object val, Type conversion)
                                        {
                                            var t = conversion;
                                            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                                            {
                                                if (val == null)
                                                {
                                                    return null;
                                                }

                                                t = Nullable.GetUnderlyingType(t);
                                            }

                                            return Convert.ChangeType(val, t);
                                        }

                                        value = ChangeType(valueString, prop.PropertyType);
                                    }
                                    catch (ArgumentException)
                                    {
                                        // Programmer mistake
                                        throw new InvalidOperationException($"The filter value '{valueString}' could not be parsed into a valid {propType}");
                                    }
                                }

                                var constant = Expression.Constant(value, prop.PropertyType);

                                // (C) parse the operator (e.g. "eq")
                                var op = pieces[1];
                                op = op?.ToLower() ?? "";
                                switch (op)
                                {
                                    case "gt":
                                        return Expression.GreaterThan(memberAccess, constant);

                                    case "ge":
                                        return Expression.GreaterThanOrEqual(memberAccess, constant);

                                    case "lt":
                                        return Expression.LessThan(memberAccess, constant);

                                    case "le":
                                        return Expression.LessThanOrEqual(memberAccess, constant);

                                    case "eq":
                                        return Expression.Equal(memberAccess, constant);

                                    case "ne":
                                        return Expression.NotEqual(memberAccess, constant);

                                    case "contains":
                                        return memberAccess.Contains(constant);

                                    case "ncontains":
                                        return Expression.Not(memberAccess.Contains(constant));

                                    default:
                                        throw new InvalidOperationException($"The filter operator '{op}' is not recognized");
                                }
                            }
                        }

                        return result;
                    }

                    // Programmer mistake
                    throw new Exception("Unknown AST type");
                }

                var expression = ToExpression(ast);
                var lambda = Expression.Lambda<Func<TModel, bool>>(expression, eParam);


                //////// (5) Apply lambda to the query

                query = query.Where(lambda);
            }

            return query;
        }

        /// <summary>
        /// Some controllers may wish to define their own way of handling atomic filter 
        /// query parameter expressions, overriding this virtual method is the way to do it
        /// </summary>
        protected virtual Expression ParseSpecialFilterKeyword(string keyword, ParameterExpression param)
        {
            // This method is overridden by controllers to provide special keywords that represent certain
            // complicated linq expressions that cannot be expressed with normal ODATA filter

            // Any type that contains a CreatedBy property defines a keyword "CreatedByMe"
            if (keyword == "CreatedByMe")
            {
                var createdByProperty = typeof(TModel).GetProperty("CreatedById");
                if (createdByProperty != null)
                {
                    var me = Expression.Constant(_userService.GetDbUser().Id.Value, typeof(int));
                    return Expression.Equal(Expression.Property(param, createdByProperty), me);
                }
            }

            return null;
        }

        /// <summary>
        /// Orders the query as per the orderby and desc arguments
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query">The base query to order</param>
        /// <param name="orderby">The orderby parameter which has the format 'A/B/C'</param>
        /// <param name="desc">True for a descending order</param>
        /// <returns>Ordered query</returns>
        protected virtual IQueryable<TModel> OrderBy(IQueryable<TModel> query, string orderby, bool desc)
        {
            Type modelType = typeof(TModel);
            if (!string.IsNullOrWhiteSpace(orderby))
            {
                var steps = orderby.Split('/');

                // Validate that the steps represent a valid train of navigation properties
                {
                    PropertyInfo prop = null;
                    Type propType = modelType;
                    foreach (var step in steps)
                    {
                        prop = propType.GetProperty(step);
                        if (prop == null)
                        {
                            throw new InvalidOperationException(
                                $"The property '{step}' is not a navigation property of entity type '{propType.Name}'. " +
                                $"The orderby parameter should have the general format: 'orderby=A/B'");
                        }

                        var isCollection = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>);
                        if (isCollection)
                        {
                            throw new InvalidOperationException(
                                $"The property '{step}' is a collection navigation property of type '{propType.Name}'. " +
                                $"Collection properties cannot be used in the orderby argument");
                        }

                        propType = prop.PropertyType;
                    }
                }

                // Create the key selector dynamically using LINQ expressions and apply it on the query
                {
                    var param = Expression.Parameter(modelType);
                    Expression exp = param;
                    Type propType = modelType;
                    foreach (var step in steps)
                    {
                        var prop = propType.GetProperty(step);
                        propType = prop.PropertyType;
                        exp = Expression.Property(exp, prop);

                        if (step == steps[steps.Length - 1])
                        {
                            exp = Expression.Convert(exp, typeof(object)); // To handle unboxing of e.g. int members
                        }
                    }

                    var keySelector = Expression.Lambda<Func<TModel, object>>(exp, param);

                    // Order the query taking into account the "isDescending" parameter
                    query = desc ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
                }
            }
            else
            {
                query = DefaultOrder(query);
            }

            return query;
        }

        protected virtual IQueryable<TModel> DefaultOrder(IQueryable<TModel> query)
        {
            var id = typeof(TModel).GetProperty("Id");
            if (id != null && id.PropertyType == typeof(int))
            {
                var p = Expression.Parameter(typeof(TModel), "e");
                var access = Expression.Property(p, id);
                var lambda = Expression.Lambda<Func<TModel, int>>(access, p);
                return query.OrderByDescending(lambda);
            }

            return query;
        }

        /// <summary>
        /// Includes in the query all navigation properties specified in the expand parameter
        /// </summary>
        /// <param name="query">The base query on which to include related properties</param>
        /// <param name="expand">The expand parameter which has the format 'A,B/C,D''</param>
        /// <returns>Expanded query</returns>
        protected virtual IQueryable<TModel> Expand(IQueryable<TModel> query, string expand)
        {
            // Apply the expand, which has the general format 'Expand=A,B/C,D'
            if (!string.IsNullOrWhiteSpace(expand))
            {
                var paths = expand.Split(',').Select(e => e.Trim()).Where(e => !string.IsNullOrWhiteSpace(e));
                Type modelType = typeof(TModel);
                foreach (var path in paths)
                {
                    // Validate each step in the path
                    {
                        var steps = path.Split('/');
                        PropertyInfo prop = null;
                        Type propType = modelType;
                        foreach (var step in steps)
                        {
                            prop = propType.GetProperty(step);
                            if (prop == null)
                            {
                                throw new InvalidOperationException(
                                    $"The property '{step}' is not a navigation property of entity type '{propType.Name}'. " +
                                    $"The expand argument should have the general format: 'Expand=A,B/C,D'");
                            }

                            var isCollection = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>);
                            propType = isCollection ? prop.PropertyType.GenericTypeArguments[0] : prop.PropertyType;
                        }
                    }

                    // Include
                    {
                        var includePath = path.Replace("/", ".");
                        query = query.Include(includePath);
                    }
                }
            }

            return query;
        }

        /// <summary>
        /// For every model in the list, the method will traverse the object graph and group all related
        /// models it can find (navigation properties) into a dictionary, after mapping them to their DTOs
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        protected virtual Dictionary<string, IEnumerable<DtoBase>> FlattenRelatedEntities(List<TModel> models, string expand)
        {
            var mainCollection = models.ToHashSet();

            // An inline function that recursively traverses the model tree and adds all entities
            // that have a base type of Model.ModelForSaveBase to the provided HashSet
            void Flatten(M.ModelBase model, HashSet<M.ModelBase> accRelatedModels)
            {
                foreach (var prop in model.GetType().GetProperties())
                {
                    // Navigation property

                    if (prop.GetValue(model) is M.ModelBase relatedModel && !accRelatedModels.Contains(relatedModel) && !mainCollection.Contains(relatedModel))
                    {
                        accRelatedModels.Add(relatedModel);
                        Flatten(relatedModel, accRelatedModels);
                    }


                    // Navigation collection
                    var isCollection = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>);
                    if (isCollection && prop.PropertyType.GetGenericArguments()[0].IsSubclassOf(typeof(M.ModelBase)))
                    {
                        var collection = prop.GetValue(model);
                        if (collection != null)
                        {
                            var enumerator = collection.GetType().GetMethod("GetEnumerator").Invoke(collection, new object[0]);
                            // while(e.MoveNext())
                            var moveNextMethod = enumerator.GetType().GetMethod("MoveNext");
                            var currentProp = enumerator.GetType().GetProperty("Current");
                            while ((bool)moveNextMethod.Invoke(enumerator, new object[0]))
                            {
                                var line = (M.ModelBase)currentProp.GetValue(enumerator);
                                Flatten(line, accRelatedModels);
                            }
                        }
                    }
                }
            }

            var relatedModels = new HashSet<M.ModelBase>();
            foreach (var model in models)
            {
                Flatten(model, relatedModels);
            }

            var relatedDtos = relatedModels.Select(e => _mapper.Map<DtoBase>(e));
            // This groups the related entities by collection name
            var relatedEntities = relatedDtos.GroupBy(e => GetCollectionName(e.GetType()))
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            return relatedEntities;
        }

        /// <summary>
        /// Retrieves the collection name from the DTO type
        /// </summary>
        protected static string GetCollectionName(Type dtoType)
        {
            if (!_getCollectionNameCache.ContainsKey(dtoType))
            {
                string collectionName;
                var attribute = dtoType.GetCustomAttributes<CollectionNameAttribute>(inherit: true).FirstOrDefault();
                if (attribute != null)
                {
                    collectionName = attribute.Name;
                }
                else
                {
                    collectionName = dtoType.Name;
                }

                _getCollectionNameCache[dtoType] = collectionName;
            }

            return _getCollectionNameCache[dtoType];
        }

        /// <summary>
        /// Syntactic sugar that maps a collection based on the implementation of its 'list' overload
        /// </summary>
        protected TDto Map(TModel model)
        {
            return Map(new List<TModel>() { model }).Single();
        }

        /// <summary>
        /// Syntactic sugar that flattens a single model, based on the implementation of its 'list' overload
        /// </summary>
        protected Dictionary<string, IEnumerable<DtoBase>> FlattenRelatedEntities(TModel model, string expand)
        {
            return FlattenRelatedEntities(new List<TModel> { model }, expand);
        }

        /// <summary>
        /// Changes the DateTimeOffset into a DateTime in the local time of the user suitable for exporting
        /// </summary>
        protected DateTime? ToExportDateTime(DateTimeOffset? offset)
        {
            if (offset == null)
            {
                return null;
            }

            var timeZone = TimeZoneInfo.Local;  // TODO: Use the user time zone 
            return TimeZoneInfo.ConvertTime(offset.Value, timeZone).DateTime;
        }

        /// <summary>
        /// Returns the default format for dates and date times
        /// </summary>
        protected string ExportDateTimeFormat(bool dateOnly)
        {
            return dateOnly ? "yyyy-MM-dd" : "yyyy-MM-dd hh:mm";
        }

        // Private methods

        private FileResult AbstractGridToFileResult(AbstractDataGrid abstractFile, string format)
        {
            // Get abstract grid

            FileHandlerBase handler;
            string contentType;
            if (format == FileFormats.Xlsx)
            {
                handler = new ExcelHandler(_localizer);
                contentType = MimeTypes.Xlsx;
            }
            else if (format == FileFormats.Csv)
            {
                handler = new CsvHandler(_localizer);
                contentType = MimeTypes.Csv;
            }
            else
            {
                throw new FormatException(_localizer["Error_UnknownFileFormat"]);
            }

            var fileStream = handler.ToFileStream(abstractFile);
            return File(((MemoryStream)fileStream).ToArray(), contentType);
        }
    }
}
