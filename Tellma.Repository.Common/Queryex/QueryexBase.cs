using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tellma.Repository.Common.Queryex
{
    /// <summary>
    /// The base class for all entities that are the nodes of the abstract syntax tree (AST)
    /// resulting from compiling a query expression. Equality of <see cref="QueryexBase"/>s behaves
    /// like value types, so if two <see cref="QueryexBase"/>s contain the same values they are
    /// considered equal. This class also contains static methods for parsing string expressions.
    /// <para/>
    /// Note: "Queryex" is a shortened version of "Query Expression", à la "Regex".
    /// </summary>
    public abstract class QueryexBase
    {
        protected const string FALSE = "(0 = 1)";
        protected const string TRUE = "(1 = 1)";

        #region Direction

        /// <summary>
        /// For expressions that are postfixed with a direction "desc" or "asc".
        /// </summary>
        public QxDirection Direction { get; set; }

        /// <summary>
        /// Whether or not the direction is ascending.
        /// </summary>
        public bool IsAscending => Direction == QxDirection.Asc;

        /// <summary>
        /// Whether or not the direction is descending.
        /// </summary>
        public bool IsDescending => Direction == QxDirection.Desc;

        #endregion

        #region Helper Functions

        /// <summary>
        /// Compiles the expression to the first <see cref="QxType"/> other than boolean that it can be compiled to.
        /// </summary>
        public string CompileToNonBoolean(QxCompilationContext ctx)
        {
            var (nativeSql, nativeType, _) = CompileNative(ctx);
            if (nativeType == QxType.Boolean)
            {
                throw new QueryException($"Expression {this} cannot be a {QxType.Boolean}.");
            }

            return nativeSql;
        }

        /// <summary>
        /// Compiles the expression to a boolean SQL, throws an exception if the expression cannot be compiled to boolean.
        /// </summary>
        public string CompileToBoolean(QxCompilationContext ctx)
        {
            if (TryCompile(QxType.Boolean, ctx, out string result, out QxNullity nullity))
            {
                if (nullity != QxNullity.NotNull)
                {
                    // Developer mistake
                    throw new InvalidOperationException($"[Bug] nullable boolean expression {this}.");
                }

                return result;
            }
            else
            {
                throw new QueryException($"Expression {this} could not be interpreted as a {QxType.Boolean}.");
            }
        }

        /// <summary>
        /// Returns every <see cref="QueryexFunction"/> that is one of the aggregation functions.
        /// </summary>
        public IEnumerable<QueryexFunction> Aggregations()
        {
            if (this is QueryexFunction func && func.IsAggregation)
            {
                yield return func;
            }


            foreach (var child in Children)
            {
                foreach (var ca in child.Aggregations())
                {
                    yield return ca;
                }
            }
        }

        /// <summary>
        /// Returns every <see cref="QueryexColumnAccess"/> within this expression.
        /// </summary>
        public IEnumerable<QueryexColumnAccess> ColumnAccesses()
        {
            if (this is QueryexColumnAccess columnAccess)
            {
                yield return columnAccess;
            }

            foreach (var child in Children)
            {
                foreach (var ca in child.ColumnAccesses())
                {
                    yield return ca;
                }
            }
        }

        /// <summary>
        /// Determines if any aggregations functions like Sum or Count are present within the expression tree.
        /// </summary>
        public bool ContainsAggregations => Aggregations().Any();

        /// <summary>
        /// Determines if column accesses like Line.Memo are present within the expression tree.
        /// </summary>
        public bool ContainsColumnAccesses => ColumnAccesses().Any();

        /// <summary>
        /// Returns true if the current node is one of the 5 aggregation functions (sum, count, etc...).
        /// </summary>
        public bool IsAggregation => this is QueryexFunction func && func.Name?.ToLower() switch
        {
            "sum" => true,
            "count" => true,
            "avg" => true,
            "max" => true,
            "min" => true,
            _ => false,
        };

        /// <summary>
        /// Retrieves from the expression tree every <see cref="QueryexColumnAccess"/> that
        /// has no aggregation function as one of its ancestors.
        /// </summary>
        public IEnumerable<QueryexColumnAccess> UnaggregatedColumnAccesses()
        {
            return UnaggregatedColumnAccessesInner(false);
        }

        /// <summary>
        /// Helper function for <see cref="UnaggregatedColumnAccesses"/>.
        /// </summary>
        private IEnumerable<QueryexColumnAccess> UnaggregatedColumnAccessesInner(bool aggregated)
        {
            if (IsAggregation)
            {
                aggregated = true;
            }
            else if (!aggregated && this is QueryexColumnAccess columnAccess)
            {
                yield return columnAccess;
            }

            foreach (var child in Children)
            {
                foreach (var ca in child.UnaggregatedColumnAccessesInner(aggregated))
                {
                    yield return ca;
                }
            }
        }

        /// <summary>
        /// Deep clones the entire <see cref="QueryexBase"/> tree. 
        /// <para/>
        /// This method accepts an optional parameter "prefix". When supplied, any <see cref="QueryexColumnAccess"/> 
        /// in the original tree whose path starts with this prefix will be cloned into one with that prefix removed.
        /// For example if the prefix is ["A", "B"], and a <see cref="QueryexColumnAccess"/> is present in the tree with
        /// a path of ["A", "B", "C"], the resulting <see cref="QueryexColumnAccess"/> will have a path of ["C"].
        /// </summary>
        /// <returns>A deep clone of the original tree</returns>
        public abstract QueryexBase Clone(string[] prefixToRemove = null);

        #endregion

        #region Abstract Members

        /// <summary>
        /// Attempts to compile the expression to SQL given a specific target type.
        /// </summary>
        /// <param name="targetType">The type to compile the expression</param>
        /// <param name="ctx">The <see cref="QxCompilationContext"/></param>
        /// <param name="resultSql">The result of the compilation (or null if the compilation fails)</param>
        /// <param name="resultNullity">The <see cref="QxNullity"/> of the result (or default if the compilation fails)</param>
        /// <returns>True if the compilation to the target type succeeds, false otherwise.</returns>
        public virtual bool TryCompile(QxType targetType, QxCompilationContext ctx, out string resultSql, out QxNullity resultNullity)
        {
            // Some types can be imlicitly converted to other types universally
            // Null -> Any type other than boolean
            // Bit -> Numeric
            // Bit -> Boolean

            var (nativeSql, nativeType, nativeNullity) = CompileNative(ctx);
            if (nativeType == targetType)
            {
                // Target type was not specified, or is equal to the native type
                resultNullity = nativeNullity;
                resultSql = nativeSql;
                return true;
            }
            else if (nativeType == QxType.Null)
            {
                // A native type of NULL can be interpreted as anything except boolean
                if (targetType != QxType.Boolean)
                {
                    resultNullity = nativeNullity;
                    resultSql = nativeSql;
                    return true;
                }
            }
            else if (nativeType == QxType.Bit)
            {
                // A native type of BIT can also be interpreted as a numeric or a boolean
                if (targetType == QxType.Numeric)
                {
                    resultNullity = nativeNullity;
                    resultSql = nativeSql;
                    return true;
                }
                else if (targetType == QxType.Boolean)
                {
                    resultNullity = QxNullity.NotNull;
                    resultSql = nativeNullity == QxNullity.Nullable ? $"({nativeSql} IS NOT NULL AND {nativeSql} = 1)" : $"({nativeSql} = 1)";
                    return true;
                }
            }

            // In the general case, no other implicit casting is possible
            resultSql = null;
            resultNullity = default;
            return false;
        }

        /// <summary>
        /// Compiles the expression to SQL in its native datatype
        /// </summary>
        public abstract (string sql, QxType type, QxNullity nullity) CompileNative(QxCompilationContext ctx);

        /// <summary>
        /// All the <see cref="QueryexBase"/> nodes that descend from the current node.
        /// </summary>
        public virtual IEnumerable<QueryexBase> Children
        {
            get
            {
                yield break;
            }
        }

        #endregion

        #region Symbols & Operators

        /// <summary>
        /// The symbols recognized by the tokenizer. Note: The order is important, since we take the first match.
        /// So >= should be listed before =
        /// </summary>
        static readonly List<string> _symbols = new(new string[] { // the order matters
            
                    // Comparison Operators
                    "!=", "<>", "<=", ">=", "<", ">", "=", 

                    // Logical Operators
                    "&&", "||", "!",

                    // Brackets and comma
                    "(", ")", ",",
            
                    // Arithmetic Operators
                    "+", "-", "*", "/", "%",

                    // String Operators (for backward compatibility)
                    "contains", "startsw", "endsw", 
                
                    // Tree Operators (for backward compatibility)
                    "descof",

                    // Logical Operators (for backward compatibility)
                    "not", "and", "or",
            
                    // Comparison Operators (for backward compatibility)
                    "gt", "ge", "lt", "le", "eq", "ne",

                    // Directions
                    "asc", "desc",
                });

        /// <summary>
        /// This list contains the precedence and associativity of supported operators (that do not require brackets)
        /// The precedences used are the same as T-SQL (https://bit.ly/2YnyfbV).
        /// </summary>
        private static readonly Dictionary<string, OperatorInfo> _operatorInfos = new(StringComparer.OrdinalIgnoreCase)
        {
            // Arithmetic Opreators (take 2 numbers and return a number)
            ["*"] = new OperatorInfo { Precedence = 2, Associativity = Associativity.Left },
            ["/"] = new OperatorInfo { Precedence = 2, Associativity = Associativity.Left },
            ["%"] = new OperatorInfo { Precedence = 2, Associativity = Associativity.Left },
            ["+"] = new OperatorInfo { Precedence = 3, Associativity = Associativity.Left },
            ["-"] = new OperatorInfo { Precedence = 3, Associativity = Associativity.Left }, // Same precedence as unary or binary

            // Comparison Operators (take 2 objects and return a boolean)
            ["="] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            ["!="] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            ["<>"] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            ["<="] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            ["<"] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            [">"] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            [">="] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            ["eq"] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            ["ne"] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            ["le"] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            ["lt"] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            ["gt"] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            ["ge"] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },

            // Infix functions
            ["contains"] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            ["startsw"] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            ["endsw"] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            ["descof"] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },

            // Logical Operators
            ["!"] = new OperatorInfo { Precedence = 5, Associativity = Associativity.Left },
            ["&&"] = new OperatorInfo { Precedence = 6, Associativity = Associativity.Left },
            ["||"] = new OperatorInfo { Precedence = 7, Associativity = Associativity.Left },
            ["not"] = new OperatorInfo { Precedence = 5, Associativity = Associativity.Left },
            ["and"] = new OperatorInfo { Precedence = 6, Associativity = Associativity.Left },
            ["or"] = new OperatorInfo { Precedence = 7, Associativity = Associativity.Left },
        };

        /// <summary>
        /// Returns true if the symbol is alphabetical and may throw off the Lexer, these require special handling.
        /// </summary>
        private static bool IsAlphabeticSymbol(string op)
        {
            return op switch
            {
                "eq" or "ne" or "le" or "lt" or "gt" or "ge" or
                "contains" or "startsw" or "endsw" or "descof" or
                "not" or "and" or "or" or "asc" or "desc" => true,
                _ => false,
            };
        }

        /// <summary>
        /// Returns true if the operator is a valid unary operator like "-" and "not".
        /// </summary>
        private static bool ValidUnaryOperator(string op)
        {
            return op.ToLower() switch
            {
                "-" or "+" or "!" or "not" => true,
                _ => false,
            };
        }

        /// <summary>
        /// Returns true for operators that can be used.
        /// </summary>
        /// <remarks>
        /// Note: The function does not check if it's a valid operator in the first place.
        /// </remarks>
        private static bool ValidBinaryOperator(string op)
        {
            return op.ToLower() switch
            {
                "!" or "not" => false,
                _ => true,
            };
        }

        /// <summary>
        /// Converts the string representation of a direction to the corresponding <see cref="QxDirection"/>
        /// value. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="token">A string containing a direction to format.</param>
        /// <param name="dir">When this method returns, contains the <see cref="QxDirection"/> representation of <paramref name="token"/>.</param>
        /// <returns>True if converted successfully otherwise false.</returns>
        private static bool TryParseDirection(string token, out QxDirection dir)
        {
            dir = token?.ToLower() switch
            {
                "asc" => QxDirection.Asc,
                "desc" => QxDirection.Desc,
                _ => QxDirection.None
            };

            return dir != QxDirection.None;
        }

        #endregion

        #region Parser

        /// <summary>
        /// Parses <paramref name="expressionString"/>, which is expected to be a string of comma
        /// separated query expressions, into a list of <see cref="QueryexBase"/>. Each <see cref="QueryexBase"/>
        /// is an expression tree representing the comma separated expression.
        /// </summary>
        /// <param name="expressionString">A string of comma separated query expressions.</param>
        /// <param name="expectDirKeywords">Whether direction key words "asc" or "desc" are expected at the end of each expression in <paramref name="expressionString"/>.</param>
        /// <param name="expectPathsOnly">True if column accesses will always terminate with a navigation property.</param>
        public static IEnumerable<QueryexBase> Parse(string expressionString, bool expectDirKeywords = false, bool expectPathsOnly = false)
        {
            if (string.IsNullOrWhiteSpace(expressionString))
            {
                yield break;
            }

            IEnumerable<string> tokenStream = Tokenize(expressionString);
            foreach (var expression in ParseTokenStream(tokenStream, expressionString, expectDirKeywords, expectPathsOnly))
            {
                if (expression != null)
                {
                    yield return expression;
                }
            }
        }

        /// <summary>
        /// Performs lexical analysis on the input string.
        /// </summary>
        /// <param name="expressionString">The string to perform lexical analysis on.</param>
        /// <returns>A stream of tokens recognizable in the query expression grammer.</returns>
        private static IEnumerable<string> Tokenize(string expressionString)
        {
            char[] expArray = expressionString.ToCharArray();
            bool insideQuotes = false;
            StringBuilder acc = new();
            int index = 0;

            bool TryMatchSymbol(int i, out string matchingSymbol)
            {
                // This basically finds the first symbol that matches the beginning of the current index at filterArray
                matchingSymbol = _symbols.FirstOrDefault(symbol => (expArray.Length - i) >= symbol.Length &&
                    Enumerable.Range(0, symbol.Length).All(j => symbol[j] == char.ToLower(expArray[i + j])));

                // Some operators like "not" requires more elaborate handling, since it may not necessarily be preceded or superseded by a space
                // but we don't want to confuse it with properties that contain "not" in their name like "Notes"
                if (IsAlphabeticSymbol(matchingSymbol))
                {
                    int prevIndex = i - 1;
                    bool precededProperly = prevIndex < 0 || !QueryexColumnAccess.ProperChar(expArray[prevIndex]);
                    int nextIndex = i + matchingSymbol.Length;
                    bool followedProperly = nextIndex >= expArray.Length || !QueryexColumnAccess.ProperChar(expArray[nextIndex]);

                    if (!precededProperly || !followedProperly)
                    {
                        matchingSymbol = null;
                    }
                }

                // This is to maintain the original casing of the symbol (DescOf vs descof)
                if (matchingSymbol != null)
                {
                    var segment = new ArraySegment<char>(expArray, i, matchingSymbol.Length);
                    matchingSymbol = string.Join("", segment);

                    return true;
                }
                else
                {
                    return false;
                }
            }

            while (index < expArray.Length)
            {
                bool isSingleQuote = expArray[index] == '\'';
                if (isSingleQuote)
                {
                    bool followedBySingleQuote = (index + 1) < expArray.Length && expArray[index + 1] == '\'';

                    acc.Append(expArray[index]);
                    index++;

                    if (!insideQuotes)
                    {
                        insideQuotes = true;
                    }
                    else if (!followedBySingleQuote)
                    {
                        insideQuotes = false;
                    }
                    else // inside quotes and followed by a single quote
                    {
                        index++; // skip the other single quote, it's just there for escaping the first one
                    }
                }
                else
                {
                    // Everything that is not inside single quotes is ripe for lexical analysis   
                    if (!insideQuotes && TryMatchSymbol(index, out string matchingSymbol))
                    {
                        // Return all that has been accumulating before the symbol
                        if (acc.Length > 0)
                        {
                            var token = acc.ToString().Trim();
                            if (!string.IsNullOrWhiteSpace(token))
                            {
                                yield return token;
                            }

                            acc.Clear();
                        }

                        // Return the symbol  
                        yield return matchingSymbol.Trim();

                        index += matchingSymbol.Length;
                    }
                    else
                    {
                        acc.Append(expArray[index]);
                        index++;
                    }
                }
            }

            if (insideQuotes)
            {
                throw new QueryException($"Uneven number of single quotation marks in {expressionString}, quotation marks in string literals should be escaped by specifying them twice.");
            }

            if (acc.Length > 0)
            {
                var token = acc.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    yield return token;
                }
            }
        }

        /// <summary>
        /// Parses the token stream into a collection of abstrast syntax trees.
        /// </summary>
        private static IEnumerable<QueryexBase> ParseTokenStream(IEnumerable<string> tokens, string expressionString, bool expectDirKeywords, bool expectPathsOnly)
        {
            // This is an implementation of the shunting-yard algorithm from Edsger Dijkstra https://bit.ly/1fEvvLI
            var ops = new Stack<(string op, bool isUnary)>();
            var brackets = new Stack<BracketInfo>();
            var output = new Stack<QueryexBase>();

            // Inline function to pop from the ops stack and apply to the output
            void PopOperatorToOutput()
            {
                var (op, usedAsUnaryOperator) = ops.Pop();

                QueryexBase exp;
                if (usedAsUnaryOperator)
                {
                    if (!ValidUnaryOperator(op))
                    {
                        // * OR * 3
                        throw new QueryException($"Infix operator '{op}' is missing its first operand.");
                    }
                    else if (output.Count < 1)
                    {
                        // !
                        throw new QueryException($"Unary operator '{op}' is missing its operand.");
                    }
                    else
                    {
                        var inner = output.Pop();
                        exp = new QueryexUnaryOperator(op: op, inner);
                    }
                }
                else
                {
                    // Binary operator (since we don't have postfix operators)
                    if (!ValidBinaryOperator(op))
                    {
                        // 2 ! 3
                        throw new QueryException($"Unary Operator '{op}' is used like an infix operator.");
                    }
                    else if (output.Count < 2)
                    {
                        // 3 *
                        throw new QueryException($"Infix operator '{op}' is missing its second operand.");
                    }
                    else
                    {
                        var right = output.Pop();
                        var left = output.Pop();
                        exp = new QueryexBinaryOperator(op: op, left: left, right: right);
                    }
                }

                output.Push(exp);
            }

            // Inline function to increment the argument count of the current function invocation (if any) 
            void IncrementArity()
            {
                if (brackets.Count > 0)
                {
                    // Increment the arguments counter if a comma count was incremented earlier
                    var peek = brackets.Peek();
                    if (peek.IsFunction && peek.Arity == peek.Arguments.Count)
                    {
                        peek.Arity++;
                    }
                }
            }

            // Inline function to move the function argument from the output stack safely into the brackets info (and some validation)
            void TerminateFunctionArgument(BracketInfo bracketsInfo)
            {
                if (bracketsInfo.Arity > 0)
                {
                    // Some validation
                    if (bracketsInfo.Arguments.Count != bracketsInfo.Arity - 1)
                    {
                        throw new QueryException("Blank function arguments are not allowed, pass null if that was your intention.");
                    }

                    if (output.Count == 0)
                    {
                        // The previous check should take care of this
                        throw new QueryException("[Bug] Output stack is empty.");
                    }

                    // Move the argument from the stack to the brackets info, this is in order to parse something like F(1, 2, +) correctly
                    bracketsInfo.Arguments.Add(output.Pop());
                }
            }

            // Useful variables
            bool currentTokenIsPotentialFunction = false;
            bool previousTokenIsPotentialFunction;
            bool expressionTerminated;
            string previousToken = null;

            // By inspecting the previous token we can tell if the current token is syntacitcally used
            // like a prefix unary operator or function (as opposed to binary operators)
            bool CurrentTokenUsedLikeAPrefix()
            {
                return previousToken == null || previousToken == "," || previousToken == "(" || _operatorInfos.ContainsKey(previousToken);
            }

            // Inline function called when we hit an expression separating comma or the end of the expression string
            QueryexBase TerminateCurrentExpression()
            {
                expressionTerminated = true;

                // Anything left in the operators queue, add it to the output
                while (ops.Count > 0)
                {
                    if (ops.Peek().op != "(")
                    {
                        // Add to output
                        PopOperatorToOutput();
                    }
                    else
                    {
                        // There should not be a left bracket in the stack
                        throw new QueryException($"Expression contains mismatched brackets: {expressionString}.");
                    }
                }

                // If the expression is valid, there should be exactly one item in the output stack at this stage
                if (output.Count == 0)
                {
                    return null;
                }
                else if (output.Count > 1)
                {
                    throw new QueryException($"Incorrectly formatted expression: {expressionString}.");
                }

                var res = output.Pop();

                // Set the direction if any
                if (TryParseDirection(previousToken, out QxDirection dir))
                {
                    res.Direction = dir;
                }

                return res;
            }

            foreach (var currentToken in tokens)
            {
                // Shunting-yard implementation
                previousTokenIsPotentialFunction = currentTokenIsPotentialFunction;
                currentTokenIsPotentialFunction = false;
                expressionTerminated = false;

                if (_operatorInfos.TryGetValue(currentToken, out OperatorInfo opInfo)) // if it is an operator
                {
                    // Increment the argument count if not already incremented
                    IncrementArity();

                    // Determine the operator usage: Unary (like negative sign) vs Binary (like subtraction)
                    bool usedAsUnaryOperator = CurrentTokenUsedLikeAPrefix();

                    // If binary, we pop from the operator stack according to the shunting yard alogirhtm
                    if (!usedAsUnaryOperator) // Unary operators do not pop anything
                    {
                        // Inline predicate determines how many items do we pop from the operator stack
                        bool KeepPopping()
                        {
                            /* Modified from Wikipedia: Keep popping while...

                                (the operator at the top of the operator stack is not a left paren) AND 
                                (   
                                    (there is a function at the top of the operator stack) OR
                                    (there is an operator at the top of the operator stack with greater precedence) OR
                                    (the operator at the top of the operator stack has equal precedence and is left associative)
                                )
                             */

                            if (ops.Count == 0)
                            {
                                // There is nothing left to pop
                                return false;
                            }

                            var (opsPeek, _) = ops.Peek();
                            bool isOperator = _operatorInfos.TryGetValue(opsPeek, out OperatorInfo opsPeekInfo);
                            bool isFunction = opsPeek != "(" && !isOperator; // We only push functions, left parens and operators on the ops stack

                            return opsPeek != "(" &&
                                (
                                    isFunction ||
                                    opsPeekInfo.Precedence < opInfo.Precedence || // less than means greater precedence
                                    (opsPeekInfo.Precedence == opInfo.Precedence && opsPeekInfo.IsLeftAssociative)
                                );
                        }

                        while (KeepPopping())
                        {
                            PopOperatorToOutput();
                        }
                    }

                    // Finally, push the token (and it's usage on the stack)
                    currentTokenIsPotentialFunction = usedAsUnaryOperator && QueryexFunction.IsValidFunctionName(currentToken);
                    ops.Push((currentToken, usedAsUnaryOperator));
                }
                else if (currentToken == "(")
                {
                    if (previousTokenIsPotentialFunction)
                    {
                        if (_operatorInfos.ContainsKey(previousToken))
                        {
                            // if the previous token is an operator it's already in the ops stack (not the output), so we do nothing
                        }
                        else
                        {
                            // Else the previous token was added to the output incrrectly: remove it
                            output.Pop();
                            ops.Push((previousToken, false)); // Add it as a function to the ops stack instead (isUnary doesn't matter)
                        }
                    }
                    else
                    {
                        // Not a function call
                        IncrementArity();
                    }

                    brackets.Push(new BracketInfo(isFunction: previousTokenIsPotentialFunction));
                    ops.Push((currentToken, false));
                }
                else if (currentToken == ")")
                {
                    // Keep popping from the operator queue until you hit a left paren
                    while (ops.Count > 0 && ops.Peek().op != "(")
                    {
                        // Add to output
                        PopOperatorToOutput();
                    }

                    if (ops.Count > 0 && ops.Peek().op == "(")
                    {
                        // Pop the left paren
                        ops.Pop();
                        var bracketsInfo = brackets.Pop();
                        if (bracketsInfo.IsFunction)
                        {
                            // Terminate the final argument
                            TerminateFunctionArgument(bracketsInfo);

                            var (functionName, _) = ops.Pop();

                            // Add the function to the output
                            var func = new QueryexFunction(name: functionName, args: bracketsInfo.Arguments.ToArray());

                            if (func.IsAggregation && func.Children.Any(e => e.ContainsAggregations))
                            {
                                throw new QueryException($"The expression {func} contains an aggregation within an aggregation.");
                            }

                            output.Push(func);
                        }
                        else if (previousToken == "(")
                        {
                            throw new QueryException("Invalid empty brackets ().");
                        }
                    }
                    else
                    {
                        // There should have been a left paren in the stack
                        throw new QueryException($"Expression contains mismatched brackets.");
                    }
                }
                else if (currentToken == ",")
                {
                    if (brackets.Count == 0)
                    {
                        // This is a top level comma separating expression atoms: E1,E2,E3
                        yield return TerminateCurrentExpression();
                    }
                    else if (brackets.Peek().IsFunction) // A comma in a function invocation
                    {
                        // Keep popping from the operator queue until you hit the left bracket
                        while (ops.Count > 0 && ops.Peek().op != "(")
                        {
                            // Add to output
                            PopOperatorToOutput();
                        }

                        var bracketsInfo = brackets.Peek();

                        // Terminate the current argument
                        TerminateFunctionArgument(bracketsInfo);
                    }
                    else
                    {
                        throw new QueryException("Unexpected comma ',' character. Commas are only used to separate function arguments: Func(arg1, arg2, arg3).");
                    }
                }
                else if (TryParseDirection(currentToken, out _))
                {
                    // Handled in the next step
                    if (!expectDirKeywords)
                    {
                        throw new QueryException($"Unexpected keyword '{currentToken}' in expression: {expressionString}");
                    }
                }
                else
                {
                    IncrementArity();

                    // Flag it if potential function
                    currentTokenIsPotentialFunction = CurrentTokenUsedLikeAPrefix() && QueryexFunction.IsValidFunctionName(currentToken);

                    // It's (hopefully) a simple atom => add it the output
                    // IF this is a valid function name and the very next token is an opening bracket "(" then
                    // this is a function invocation, this action is corrected
                    // by popping from the output and pushing in ops
                    QueryexBase exp;
                    var tokenLower = currentToken.ToLower();
                    switch (tokenLower)
                    {
                        case "null":
                            exp = new QueryexNull();
                            break;

                        case "true":
                        case "false":
                            exp = new QueryexBit(value: tokenLower == "true");
                            break;

                        case "me":
                        case "today":
                        case "now":
                            exp = new QueryexFunction(name: tokenLower);
                            break;

                        default:
                            if (QueryexQuote.IsValidQuote(currentToken, out string quoteValue))
                            {
                                exp = new QueryexQuote(value: quoteValue);
                            }
                            else if (QueryexNumber.IsValidNumber(currentToken, out decimal decimalValue, out int decimals))
                            {
                                exp = new QueryexNumber(value: decimalValue, decimals: decimals);
                            }
                            else if (QueryexColumnAccess.IsValidColumnAccess(currentToken, expectPathsOnly, out string[] pathArray, out string propName))
                            {
                                exp = new QueryexColumnAccess(path: pathArray, prop: propName);
                            }
                            else
                            {
                                throw new QueryException($"Unrecognized token: {currentToken}");
                            }
                            break;
                    }

                    output.Push(exp);
                }

                // If the last token was a desc or asc keyword, then this token has to terminate the current atom, 
                if (TryParseDirection(previousToken, out _) && !expressionTerminated)
                {
                    var keyword = previousToken?.ToLower();
                    throw new QueryException($"Keyword '{keyword}' must come after the expression and outside any brackets like this: <exp1> {keyword}, <exp2> {keyword}.");
                }

                previousToken = currentToken;
            }

            yield return TerminateCurrentExpression();
        }

        #region Helper Types

        /// <summary>
        /// Represents a pair of brackets in an expression. A pair of brackets can be for surrounding the arguments of a function.
        /// </summary>
        private class BracketInfo
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public BracketInfo(bool isFunction)
            {
                IsFunction = isFunction;
            }

            /// <summary>
            /// True if the brackets are those surrounding the arguments of a function, false if
            /// they are regular brackets to override the default associativity.
            /// </summary>
            public bool IsFunction { get; }

            #region For Functions

            /// <summary>
            /// The number of arguments of the function.
            /// </summary>
            public int Arity { get; set; }

            /// <summary>
            /// The arguments of the function (kept here off the output stack to prevent an
            /// expression like "F(1, 2, +)" from evaluating incorrectly).
            /// </summary>
            public List<QueryexBase> Arguments { get; set; } = new List<QueryexBase>();

            #endregion
        }

        /// <summary>
        /// Used internally to store the precedence and associativity of a binary operator
        /// </summary>
        private struct OperatorInfo
        {
            /// <summary>
            /// https://bit.ly/3btXHkj
            /// </summary>
            public int Precedence { get; set; }

            /// <summary>
            /// https://bit.ly/2Kp2Yvl
            /// </summary>
            public Associativity Associativity { get; set; }

            /// <summary>
            /// https://bit.ly/3tPfQzg
            /// </summary>
            public bool IsLeftAssociative => Associativity == Associativity.Left;
        }

        /// <summary>
        /// https://bit.ly/2Kp2Yvl
        /// </summary>
        private enum Associativity
        {
            Left,
            Right
        }

        #endregion

        #endregion
    }
}
