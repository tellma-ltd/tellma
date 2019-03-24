using BSharp.Services.MultiTenancy;
using BSharp.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BSharp.Services.FilterParser
{
    public class FilterParser : IFilterParser
    {
        private readonly ITenantUserInfoAccessor _info;

        public FilterParser(ITenantUserInfoAccessor info)
        {
            _info = info;
        }

        /// <summary>
        /// Parses the OData like filter expression into a linq lambda expression
        /// </summary>
        public Expression ParseFilterExpression<T>(string filter, ParameterExpression eParam, int? currentUserId = null, TimeZoneInfo currentUserTimeZone = null)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (eParam == null)
            {
                throw new ArgumentNullException(nameof(eParam));
            }

            // The usual steps of compilers:
            // 1 - Preprocessing
            // 2 - Lexical Analysis (Tokenization)
            // 3 - Compile into an Abstract Expression Tree (AST)
            Ast ast = Abstract(Tokenize(Preprocess(filter)));

            // 4 - Compile the AST into an Expression
            currentUserId = currentUserId ?? _info.GetCurrentInfo()?.UserId;
            // currentUserTimeZone = currentUserTimeZone; // TODO user current user time zone
            return Express<T>(ast, eParam, currentUserId, currentUserTimeZone);
        }

        /// <summary>
        /// Extracts the list of paths from the OData like filter expression
        /// for example given the filter expression: "A/B eq v and C eq 3"
        /// the result would be ["A/B", "C"]
        /// </summary>
        public List<string> ExtractPaths(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                throw new ArgumentNullException(nameof(filter));
            }

            Ast ast = Abstract(Tokenize(Preprocess(filter)));
            return ExtractPaths(ast);
        }

        /// <summary>
        /// Preprocesses the filer expression: removing any duplicate spaces and trimming it
        /// </summary>
        private static string Preprocess(string filter)
        {

            // Ensure no spaces are repeated
            Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
            filter = regex.Replace(filter, " ");

            // Trim
            filter = filter.Trim();

            return filter;
        }

        /// <summary>
        /// Lexical Analysis: Turns the filter expression into a stream of recognized tokens
        /// </summary>
        private static List<string> Tokenize(string preprocessedFilter)
        {
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
            while (index < preprocessedFilter.Length)
            {
                // Lexical analysis ignores what's inside single quotes
                if (preprocessedFilter[index] == '\'' && (index == 0 || preprocessedFilter[index - 1] != '\'') && (index == preprocessedFilter.Length - 1 || preprocessedFilter[index + 1] != '\''))
                {
                    insideQuotes = !insideQuotes;
                    acc += preprocessedFilter[index];
                    index++;
                }
                else if (insideQuotes)
                {
                    acc += preprocessedFilter[index];
                    index++;
                }
                else
                {
                    // Everything that is not inside single quotes is ripe for lexical analysis      
                    var matchingSymbol = symbols.FirstOrDefault(preprocessedFilter.Substring(index).StartsWith);
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
                        acc += preprocessedFilter[index];
                        index++;
                    }
                }
            }

            if (insideQuotes)
            {
                // Programmer mistake
                throw new InvalidOperationException("Uneven number of single quotation marks in filter query parameter, quotation marks in literals should be escaped by specifying them twice");
            }

            if (!string.IsNullOrWhiteSpace(acc))
            {
                tokens.Add(acc.Trim());
            }

            return tokens;
        }

        /// <summary>
        /// Constructs and returns an abstract expression tree based on a token stream
        /// </summary>
        private static Ast Abstract(List<string> tokens)
        {
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
                    throw new InvalidOperationException("Badly formatted filter parameter");
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
            return ast;
        }

        /// <summary>
        /// Compiles the AST into a lambda expression
        /// </summary>
        /// <typeparam name="T">The type of the principle entity on which the lambda operators</typeparam>
        /// <param name="ast">The abstract expression tree to be compiled</param>
        /// <param name="eParam">The parameter expression to be used in the lambda</param>
        /// <param name="currentUserId">The Id of the user doing the filtering</param>
        /// <param name="currentUserTimeZone">The time zone of the user doing the filtering</param>
        /// <returns>The complie lambda expression</returns>
        private static Expression Express<T>(Ast ast, ParameterExpression eParam, int? currentUserId = null, TimeZoneInfo currentUserTimeZone = null)
        {
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
                    var modelType = typeof(T);
                    var v = atom.Value;

                    // Indicates a programmer mistake
                    if (string.IsNullOrWhiteSpace(v))
                    {
                        throw new InvalidOperationException("An atomic expression cannot be empty");
                    }

                    // Atoms come in the following form: Path op Value, for example: Address/Street eq 'Huntington Rd.'
                    var pieces = v.Split(" ");
                    if (pieces.Length != 3)
                    {
                        throw new InvalidOperationException($"One of the atomic expressions ({v}) does not have the valid form: 'Path op Value'");
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

                        ///// Normal parsing when no special parsing is available
                        // (B) Parse the value (e.g. "'Huntington Rd.'")
                        var valueString = string.Join(" ", pieces.Skip(2));
                        object value;
                        if (valueString == "null")
                        {
                            value = null;
                        }
                        else if (valueString?.ToLower() == "me")
                        {
                            value = currentUserId;
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
                                value = valueString.ChangeType(prop.PropertyType, currentUserTimeZone);
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

                // Programmer mistake
                throw new Exception("Unknown AST type");
            }

            var expression = ToExpression(ast);
            return expression;

        }

        /// <summary>
        /// Extracts the list of paths from the abstract expression tree
        /// </summary>
        private static List<string> ExtractPaths(Ast ast)
        {
            List<string> paths = new List<string>();
            void Traverse(Ast tree)
            {
                if (tree is AstBrackets bracketsAst)
                {
                    Traverse(bracketsAst.Inner);
                }

                else if (tree is AstConjunction conAst)
                {
                    Traverse(conAst.Left);
                    Traverse(conAst.Right);
                }

                else if (tree is AstDisjunction disAst)
                {
                    Traverse(disAst.Left);
                    Traverse(disAst.Right);
                }

                else if (tree is AstAtom atom)
                {

                    // Indicates a programmer mistake
                    var v = atom.Value;
                    if (string.IsNullOrWhiteSpace(v))
                    {
                        throw new InvalidOperationException("An atomic expression cannot be empty");
                    }

                    // Atoms come in the following form: Path op Value, for example: Address/Street eq 'Huntington Rd.'
                    var pieces = v.Split(" ");
                    if (pieces.Length != 3)
                    {
                        throw new InvalidOperationException($"One of the atomic expressions ({v}) does not have the valid form: 'Path op Value'");
                    }
                    else
                    {
                        // (A) Parse the member access path (e.g. "Address/Street")
                        var path = pieces[0];
                        paths.Add(path);
                    }
                }
                else
                {
                    throw new Exception("Unknown AST type");
                }
            }

            Traverse(ast);

            return paths;
        }

    }
}
