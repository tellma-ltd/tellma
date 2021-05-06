using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tellma.Entities;
using Tellma.Entities.Descriptors;
using Tellma.Services.Utilities;

namespace Tellma.Data.Queries
{
    public abstract class QueryexBase
    {
        public const string FALSE = "(0 = 1)";
        public const string TRUE = "(1 = 1)";

        #region Direction

        public QxDirection Direction { get; set; }
        public bool IsAscending => Direction == QxDirection.Asc;
        public bool IsDescending => Direction == QxDirection.Desc;

        #endregion

        #region Helper Functions

        /// <summary>
        /// Compiles the expression to the first <see cref="QxType"/> other than boolean that it can be compiled to.
        /// The <see cref="QxType"/>s are tested in the order specified in <see cref="QxTypes.AllExceptBoolean"/>
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
        /// Compiles the expression to a boolean SQL, throws an exception if the expression cannot be compiled to boolean
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
        /// Returns every <see cref="QueryexColumnAccess"/> within this expression
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
        /// Determines if any aggregations functions like Sum or Count are present within the expression tree
        /// </summary>
        public bool ContainsAggregations => Aggregations().Any();

        /// <summary>
        /// Determines if a column accesses like Line.Memo are present within the expression tree
        /// </summary>
        public bool ContainsColumnAccesses => ColumnAccesses().Any();

        public bool IsAggregation => this is QueryexFunction func && func.Name?.ToLower() switch
        {
            "sum" => true,
            "count" => true,
            "avg" => true,
            "max" => true,
            "min" => true,
            _ => false,
        };

        public IEnumerable<QueryexColumnAccess> UnaggregatedColumnAccesses()
        {
            return UnaggregatedColumnAccessesInner(false);
        }

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
        /// This method accepts an optional parameter "prefix". When supplied any <see cref="QueryexColumnAccess"/> 
        /// in the original tree whose path starts with this prefix will be cloned into one with that prefix removed.
        /// For example if the prefix is ["A", "B"], and a <see cref="QueryexColumnAccess"/> is present in the tree with
        /// a path of ["A", "B", "C"], the resulting <see cref="QueryexColumnAccess"/> will have a path of ["C"]
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
        /// The precedences used are the same as T-SQL (https://bit.ly/2YnyfbV)
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

        public static bool IsAlphabeticSymbol(string op)
        {
            return op switch
            {
                "eq" or "ne" or "le" or "lt" or "gt" or "ge" or 
                "contains" or "startsw" or "endsw" or "descof" or 
                "not" or "and" or "or" or "asc" or "desc" => true,
                _ => false,
            };
        }

        private static bool ValidUnaryOperator(string op)
        {
            return op.ToLower() switch
            {
                "-" or "+" or "!" or "not" => true,
                _ => false,
            };
        }

        /// <summary>
        /// Returns true for operators that can be used 
        /// Note: The function does not check if it's a valid operator in the first place.
        /// </summary>
        private static bool ValidBinaryOperator(string op)
        {
            return op.ToLower() switch
            {
                "!" or "not" => false,
                _ => true,
            };
        }

        private static bool IsDirectionKeyword(string token, out QxDirection dir)
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

                // The operator "not" requires more elaborate handling, since it may not necessarily be preceded or superseded by a space
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
                if (IsDirectionKeyword(previousToken, out QxDirection dir))
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
                else if (IsDirectionKeyword(currentToken, out _))
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
                if (IsDirectionKeyword(previousToken, out _) && !expressionTerminated)
                {
                    var keyword = previousToken?.ToLower();
                    throw new QueryException($"Keyword '{keyword}' must come after the expression and outside any brackets like this: <exp1> {keyword}, <exp2> {keyword}.");
                }

                previousToken = currentToken;
            }

            yield return TerminateCurrentExpression();
        }

        #region Helper Types

        private class BracketInfo
        {
            public BracketInfo(bool isFunction)
            {
                IsFunction = isFunction;
            }

            public bool IsFunction { get; }

            // The rest is only for functions

            /// <summary>
            /// The number of arguments of the function
            /// </summary>
            public int Arity { get; set; }

            /// <summary>
            /// The arguments of the function (kept here off the output stack to prevent an expression like "F(1, 2, +)" from evaluating incorrectly)
            /// </summary>
            public List<QueryexBase> Arguments { get; set; } = new List<QueryexBase>();
        }

        /// <summary>
        /// Used internally to store the precedence and associativity of a binary operator
        /// </summary>
        private struct OperatorInfo
        {
            public int Precedence { get; set; }

            public Associativity Associativity { get; set; }

            public bool IsLeftAssociative
            {
                get
                {
                    return Associativity == Associativity.Left;
                }
            }
        }

        /// <summary>
        /// https://bit.ly/2Kp2Yvl
        /// </summary>
        private enum Associativity { Left, Right }

        #endregion

        #endregion
    }

    public class QueryexColumnAccess : QueryexBase
    {
        public QueryexColumnAccess(string[] path, string prop)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Property = prop;
        }

        public string[] Path { get; }

        public string Property { get; }

        public override string ToString()
        {
            var path = string.Join(".", Path) ?? "";
            var prop = Property ?? "";
            var dot = string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(prop) ? "" : ".";

            return $"{path}{dot}{prop}";
        }

        public override (string, QxType, QxNullity) CompileNative(QxCompilationContext ctx)
        {
            if (string.IsNullOrWhiteSpace(Property))
            {
                // Developer mistake
                throw new InvalidOperationException($"Bug: Invoking {nameof(CompileNative)} on a {nameof(QueryexColumnAccess)} that does not have a property.");
            }

            var propName = Property;

            // (A) Calculate Nullity (the entire path foreign keys + the final property must be all NOT NULL)
            bool pathNotNull = true;
            var join = ctx.Joins;
            foreach (var step in Path)
            {
                var navPropDesc = join.EntityDescriptor.NavigationProperty(step);
                pathNotNull = pathNotNull && navPropDesc.ForeignKey.IsNotNull;
                join = join[step];
            }

            var propDesc = join.EntityDescriptor.Property(propName);
            if (propDesc == null)
            {
                throw new QueryException($"Property '{propName}' does not exist on type {join.EntityDescriptor.Name}.");
            }

            QxNullity nullity = pathNotNull && propDesc.IsNotNull ? QxNullity.NotNull : QxNullity.Nullable;

            // (B) Calculate the type
            QxType type;
            var propType = Nullable.GetUnderlyingType(propDesc.Type) ?? propDesc.Type;
            switch (propType.Name)
            {
                case nameof(Char):
                case nameof(String):
                    type = QxType.String;
                    break;

                case nameof(Byte):
                case nameof(SByte):
                case nameof(Int16):
                case nameof(UInt16):
                case nameof(Int32):
                case nameof(UInt32):
                case nameof(Int64):
                case nameof(UInt64):
                case nameof(Single):
                case nameof(Double):
                case nameof(Decimal):
                    type = QxType.Numeric;
                    break;

                case nameof(Boolean):
                    type = QxType.Bit;
                    break;

                case nameof(DateTime):
                    type = propDesc.IncludesTime ? QxType.DateTime : QxType.Date;
                    break;

                case nameof(DateTimeOffset):
                    type = QxType.DateTimeOffset;
                    break;

                case nameof(HierarchyId):
                    type = QxType.HierarchyId;
                    break;

                case nameof(Geography):
                    type = QxType.Geography;
                    break;

                default:
                    if (propDesc is NavigationPropertyDescriptor || propDesc is CollectionPropertyDescriptor)
                    {
                        throw new QueryException($"A column access cannot terminate with a navigation property like {propDesc.Name}.");
                    }
                    else
                    {
                        // Developer mistake
                        throw new InvalidOperationException($"[Bug] Could not map type {propType.Name} to a {nameof(QxType)}"); // Future proofing
                    }
            }

            // (C) Calculate the SQL
            var sql = $"[{join.Symbol}].[{propName}]";

            // Return the result
            return (sql, type, nullity);
        }

        public override bool Equals(object exp)
        {
            return exp is QueryexColumnAccess ca &&
                ca.Property == Property &&
                ca.Path.Length == Path.Length &&
                Enumerable.Range(0, Path.Length)
                    .All(i => ca.Path[i] == Path[i]);
        }

        public override int GetHashCode()
        {
            int propCode = Property?.GetHashCode() ?? 0;

            return Path.Select(s => s.GetHashCode())
                .Aggregate(propCode, (code1, code2) => code1 ^ code2);
        }

        public override QueryexBase Clone(string[] prefix = null)
        {
            if (prefix == null || prefix.Length == 0 || !PathStartsWith(prefix))
            {
                return new QueryexColumnAccess(Path[..], Property);
            }
            else
            {
                return new QueryexColumnAccess(Path[prefix.Length..], Property);
            }
        }

        /// <summary>
        /// Helper function to check if the given path contains the same steps as the path of this <see cref="QueryexColumnAccess"/>.
        /// </summary>
        public bool PathStartsWith(string[] prefix)
        {
            return prefix != null && prefix.Length <= Path.Length &&
                Enumerable.Range(0, prefix.Length).All(i => prefix[i] == Path[i]);
        }

        public bool PathEquals(string[] path)
        {
            return path != null && path.Length == Path.Length &&
                Enumerable.Range(0, path.Length).All(i => path[i] == Path[i]);
        }

        #region Column Access Validation

        /// <summary>
        /// First character of a column access must be a letter.
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>True if the first character of the column access is valid according to the condition above, false otherwise</returns>
        private static bool ProperFirstChar(string token)
        {
            return !string.IsNullOrEmpty(token) && char.IsLetter(token[0]);
        }

        public static bool ProperChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_' || c == '.';
        }

        /// <summary>
        /// All characters of a column access must be letters, numbers, underscores or dots.
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>True if the characters the column access are valid according to the condition above, false otherwise</returns>
        private static bool ProperChars(string token)
        {
            return !string.IsNullOrEmpty(token) && token.All(ProperChar);
        }

        /// <summary>
        /// The column access must not be one of the reserved keywords
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>False if the column access is one of the keywords, true otherwise </returns>
        private static bool NotReservedKeyword(string token)
        {
            return token.ToLower() switch
            {
                "null" or "true" or "false" or "asc" or "desc" => false,
                _ => true,
            };
        }

        /// <summary>
        /// Validates the column access against all the rules: <see cref="ProperFirstChar(string)"/>,
        /// <see cref="ProperChars(string)"/> and <see cref="NotReservedKeyword(string)"/>
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>True if it passes all the validation rules, false otherwise</returns>
        public static bool IsValidColumnAccess(string token, bool expectPathsOnly, out string[] pathArray, out string propName)
        {
            bool match = ProperFirstChar(token) && ProperChars(token) && NotReservedKeyword(token);
            if (match)
            {
                var steps = token
                    .Split('.')
                    .Select(e => e.Trim())
                    .Where(e => !string.IsNullOrEmpty(e));

                if (expectPathsOnly)
                {
                    pathArray = steps.ToArray();
                    propName = null;
                }
                else
                {
                    pathArray = steps.SkipLast(1).ToArray();
                    propName = steps.Last();
                }
            }
            else
            {
                pathArray = null;
                propName = null;
            }

            return match;
        }

        #endregion
    }

    public class QueryexFunction : QueryexBase
    {
        #region Calendars

        /// <summary>
        /// Gregorian Calendar
        /// </summary>
        private const string Gregorian = "gc";

        /// <summary>
        /// Ethiopian Calendar
        /// </summary>
        private const string Ethiopian = "et";

        /// <summary>
        /// Umm Al Qura Calendar
        /// </summary>
        private const string UmAlQura = "uq";

        /// <summary>
        /// All supported calendars
        /// </summary>
        private readonly string[] SupportedCalendars = new string[] { Gregorian, Ethiopian, UmAlQura };

        #endregion

        public QueryexFunction(string name, params QueryexBase[] args)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
            Arguments = args ?? throw new ArgumentNullException(nameof(args));
        }

        public string Name { get; }

        public QueryexBase[] Arguments { get; }

        public override string ToString()
        {
            return $"{Name}({string.Join(", ", Arguments.Select(e => e.ToString().DeBracket()))})";
        }

        public override IEnumerable<QueryexBase> Children => Arguments;

        public override bool TryCompile(QxType targetType, QxCompilationContext ctx, out string resultSql, out QxNullity resultNullity)
        {
            string nameLower = Name?.ToLower();
            switch (nameLower)
            {
                case "min":
                case "max":
                    {
                        var (arg1, conditionSql) = AggregationParameters(ctx);

                        if (arg1.TryCompile(targetType, ctx, out string expSql, out resultNullity))
                        {
                            resultSql = AggregationCompile(expSql, conditionSql);
                            return true;
                        }
                        else
                        {
                            resultSql = null;
                            resultNullity = default;
                            return false;
                        }
                    }
                case "adddays":
                case "addmonths":
                case "addyears": // (date: Date | DateTime | DateTimeOffset, number: numeric) => Date | DateTime | DateTimeOffset
                    {
                        if (targetType == QxType.Date || targetType == QxType.DateTime || targetType == QxType.DateTimeOffset)
                        {
                            var (numberSql, arg2) = AddDatePartParameters(ctx, nameLower);

                            if (arg2.TryCompile(targetType, ctx, out string dateSql, out QxNullity dateNullity))
                            {
                                // Calculate the result
                                (resultSql, resultNullity) = AddDatePartCompile(nameLower, numberSql, dateSql, dateNullity);
                                return true;
                            }
                            else
                            {
                                resultSql = null;
                                resultNullity = default;
                                return false;
                            }
                        }
                        else
                        {
                            resultSql = null;
                            resultNullity = default;
                            return false;
                        }
                    }

                case "if": // (condition: boolean, value_if_true: X, value_if_false: X) => X
                    {
                        var (conditionSql, arg2, arg3) = IfParameters(ctx);

                        if (arg2.TryCompile(targetType, ctx, out string ifTrueSql, out QxNullity ifTrueNullity) &&
                            arg3.TryCompile(targetType, ctx, out string ifFalseSql, out QxNullity ifFalseNullity))
                        {
                            (resultSql, resultNullity) = IfCompile(conditionSql, ifTrueSql, ifTrueNullity, ifFalseSql, ifFalseNullity);
                            return true;
                        }
                        else
                        {
                            resultSql = null;
                            resultNullity = default;
                            return false;
                        }
                    }

                case "isnull": // (value: X, fallback_value: X) => X
                    {
                        var (arg1, arg2) = IsNullParameters();

                        if (arg1.TryCompile(targetType, ctx, out string expSql, out QxNullity expNullity) &&
                            arg2.TryCompile(targetType, ctx, out string replacementSql, out QxNullity replacementNullity))
                        {
                            (resultSql, resultNullity) = IsNullCompile(expSql, expNullity, replacementSql, replacementNullity);
                            return true;
                        }
                        else
                        {
                            resultSql = null;
                            resultNullity = default;
                            return false;
                        }
                    }
                case "today": // () => date
                    {
                        if (targetType == QxType.Date || targetType == QxType.DateTime)
                        {
                            (resultSql, resultNullity) = CompileToday(ctx, targetType);
                            return true;
                        }
                        else
                        {
                            resultSql = null;
                            resultNullity = default;
                            return false;
                        }
                    }

                default:
                    return base.TryCompile(targetType, ctx, out resultSql, out resultNullity);

            }
        }

        public override (string sql, QxType type, QxNullity nullity) CompileNative(QxCompilationContext ctx)
        {
            // The result
            string resultSql;
            QxType resultType;
            QxNullity resultNullity;

            string nameLower = Name?.ToLower();
            switch (nameLower)
            {
                case "sum":
                case "count":
                case "avg":
                case "min":
                case "max":
                    {
                        var (arg1, conditionSql) = AggregationParameters(ctx);

                        string expSql;
                        if (nameLower == "count" || nameLower == "max" || nameLower == "min")
                        {
                            // Accept anything except boolean
                            (expSql, resultType, resultNullity) = arg1.CompileNative(ctx);
                            if (resultType == QxType.Boolean)
                            {
                                throw new QueryException($"Function '{Name}': The first argument {arg1} cannot be a {QxType.Boolean} expression.");
                            }

                            if (nameLower == "count")
                            {
                                // Count always returns numeric, the other two return the same type of their argument
                                resultType = QxType.Numeric;
                            }
                        }
                        else if (arg1.TryCompile(QxType.Numeric, ctx, out expSql, out resultNullity))
                        {
                            // Accept only numeric and return only numeric
                            resultType = QxType.Numeric; // The other 3 all take numeric and return numeric
                        }
                        else
                        {
                            throw new QueryException($"Function '{Name}': The first argument {arg1} could not be interpreted as a {QxType.Numeric}.");
                        }

                        resultSql = AggregationCompile(expSql, conditionSql);
                        break;
                    }

                case "year":
                case "quarter":
                case "month":
                case "day": // (date: Date | DateTime | DateTimeOffset, calendar?: string) => numeric
                    {
                        if (Arguments.Length < 1 || Arguments.Length > 2)
                        {
                            throw new QueryException($"No overload for function '{Name}' accepts {Arguments.Length} arguments.");
                        }

                        string datePart = nameLower;

                        var arg1 = Arguments[0];
                        if (arg1.TryCompile(QxType.Date, ctx, out string dateSql, out QxNullity dateNullity) ||
                            arg1.TryCompile(QxType.DateTime, ctx, out dateSql, out dateNullity) ||
                            arg1.TryCompile(QxType.DateTimeOffset, ctx, out dateSql, out dateNullity))
                        {
                            string calendar = Gregorian; // Default
                            if (Arguments.Length >= 2)
                            {
                                var arg2 = Arguments[1];
                                if (arg2 is QueryexQuote calendarQuote)
                                {
                                    calendar = calendarQuote.Value.ToLower();
                                }
                                else
                                {
                                    throw new QueryException($"Function '{Name}': The second argument must be a simple quote like this: '{UmAlQura.ToUpper()}'.");
                                }
                            }

                            resultType = QxType.Numeric;
                            resultNullity = dateNullity;
                            resultSql = calendar switch
                            {
                                Gregorian => $"DATEPART({datePart.ToUpper()}, {dateSql.DeBracket()})", // Use SQL's built in function
                                UmAlQura => $"[wiz].[fn_UmAlQura_DatePart]('{datePart[0]}', {dateSql.DeBracket()})",
                                Ethiopian => $"[wiz].[fn_Ethiopian_DatePart]('{datePart[0]}', {dateSql.DeBracket()})",

                                _ => throw new QueryException($"Function '{Name}': The second argument {Arguments[1]} must be one of the supported calendars: '{string.Join("', '", SupportedCalendars.Select(e => e.ToUpper()))}'.")
                            };

                            break;
                        }
                        else
                        {
                            throw new QueryException($"Function '{Name}': The first argument {arg1} could not be interpreted as a {QxType.Date}, {QxType.DateTime} or {QxType.DateTimeOffset}.");
                        }
                    }

                case "weekday": // (date: Date | DateTime | DateTimeOffset) => numeric
                case "hour":
                case "minute":
                case "second": // (date: DateTime | DateTimeOffset) => numeric
                    {
                        int expectedArgCount = 1;
                        if (Arguments.Length != expectedArgCount)
                        {
                            throw new QueryException($"Function '{Name}' accepts exactly {expectedArgCount} argument(s).");
                        }

                        string datePart = nameLower;

                        // Those do not accept a QxType.Date
                        bool supportsDate = datePart == "weekday"; // Only this one accepts a date

                        var arg1 = Arguments[0];
                        if ((supportsDate && arg1.TryCompile(QxType.Date, ctx, out string dateSql, out QxNullity dateNullity)) ||
                            arg1.TryCompile(QxType.DateTime, ctx, out dateSql, out dateNullity) ||
                            arg1.TryCompile(QxType.DateTimeOffset, ctx, out dateSql, out dateNullity))
                        {
                            resultType = QxType.Numeric;
                            resultNullity = dateNullity;
                            resultSql = $"DATEPART({datePart.ToUpper()}, {dateSql.DeBracket()})";

                            break;
                        }
                        else
                        {
                            throw new QueryException($"Function '{Name}': The first argument {arg1} could not be interpreted as a {(supportsDate ? $"{QxType.Date}, " : "")}{QxType.DateTime} or {QxType.DateTimeOffset}.");
                        }
                    }

                case "addyears":
                case "addmonths":
                case "adddays": // (number: numeric, date: Date | DateTime | DateTimeOffset) => Date | DateTime | DateTimeOffset
                    {
                        var (numberSql, arg2) = AddDatePartParameters(ctx, nameLower);

                        // Argument #2 Date
                        if (arg2.TryCompile(QxType.Date, ctx, out string dateSql, out QxNullity dateNullity))
                        {
                            resultType = QxType.Date;
                        }
                        else if (arg2.TryCompile(QxType.DateTime, ctx, out dateSql, out dateNullity))
                        {
                            resultType = QxType.DateTime;
                        }
                        else if (arg2.TryCompile(QxType.DateTimeOffset, ctx, out dateSql, out dateNullity))
                        {
                            resultType = QxType.DateTimeOffset;
                        }
                        else
                        {
                            throw new QueryException($"Function '{Name}': The second argument {arg2} could not be interpreted as a {QxType.Date}, {QxType.DateTime} or {QxType.DateTimeOffset}.");
                        }

                        // Calculate the result
                        (resultSql, resultNullity) = AddDatePartCompile(nameLower, numberSql, dateSql, dateNullity);
                        break;
                    }

                case "startofyear":
                case "startofmonth":
                case "date": // (date: Date | DateTime | DateTimeOffset) => Date
                    {
                        if (Arguments.Length < 1 || Arguments.Length > 2)
                        {
                            throw new QueryException($"No overload for function '{Name}' accepts {Arguments.Length} arguments.");
                        }

                        // Argument #1: Date
                        var arg1 = Arguments[0];
                        QxType argumentType;
                        if (arg1.TryCompile(QxType.Date, ctx, out string dateSql, out QxNullity dateNullity))
                        {
                            argumentType = QxType.Date;
                        }
                        else if (arg1.TryCompile(QxType.DateTime, ctx, out dateSql, out dateNullity))
                        {
                            argumentType = QxType.DateTime;
                        }
                        else if (arg1.TryCompile(QxType.DateTimeOffset, ctx, out dateSql, out dateNullity))
                        {
                            argumentType = QxType.DateTimeOffset;
                        }
                        else
                        {
                            throw new QueryException($"Function '{Name}': The argument {arg1} could not be interpreted as a {QxType.Date}, {QxType.DateTime} or {QxType.DateTimeOffset}.");
                        }

                        // Argument #2: Calendar
                        string calendar = Gregorian; // Default
                        if (Arguments.Length >= 2)
                        {
                            var arg2 = Arguments[1];
                            if (arg2 is QueryexQuote calendarQuote)
                            {
                                calendar = calendarQuote.Value.ToLower();
                            }
                            else
                            {
                                throw new QueryException($"Function '{Name}': The second argument must be a simple quote like this: '{UmAlQura.ToUpper()}'.");
                            }
                        }

                        resultType = QxType.Date; // Always date
                        resultNullity = dateNullity;

                        switch (nameLower)
                        {
                            case "date":
                                // Date is calendar independent
                                if (argumentType == QxType.Date)
                                {
                                    resultSql = dateSql; // Return the date as is
                                }
                                else
                                {
                                    resultSql = $"CAST({dateSql} AS DATE)";
                                }
                                break;
                            case "startofmonth":
                                resultSql = calendar switch
                                {
                                    Gregorian => $"DATEADD(DAY, 1, EOMONTH({dateSql.DeBracket()}, -1))", // resultSql = $"DATEFROMPARTS(YEAR({dateSql}), MONTH({dateSql}), 1)";
                                    UmAlQura => $"[wiz].[fn_UmAlQura_StartOfMonth]({dateSql.DeBracket()})",
                                    Ethiopian => $"[wiz].[fn_Ethiopian_StartOfMonth]({dateSql.DeBracket()})",

                                    _ => throw new QueryException($"Function '{Name}': The second argument {Arguments[1]} must be one of the supported calendars: '{string.Join("', '", SupportedCalendars.Select(e => e.ToUpper()))}'.")
                                };
                                break;
                            case "startofyear":
                                resultSql = calendar switch
                                {
                                    Gregorian => $"DATEFROMPARTS(YEAR({dateSql.DeBracket()}), 1, 1)",
                                    UmAlQura => $"[wiz].[fn_UmAlQura_StartOfYear]({dateSql.DeBracket()})",
                                    Ethiopian => $"[wiz].[fn_Ethiopian_StartOfYear]({dateSql.DeBracket()})",

                                    _ => throw new QueryException($"Function '{Name}': The second argument {Arguments[1]} must be one of the supported calendars: '{string.Join("', '", SupportedCalendars.Select(e => e.ToUpper()))}'.")
                                };
                                break;
                            default:
                                throw new InvalidOperationException($"Unhandled {nameLower}");
                        }

                        break;
                    }

                //case "diffyears":
                //case "diffmonths":
                case "diffdays":
                case "diffhours":
                case "diffminutes":
                case "diffseconds": // (date1: Date | DateTime | DateTimeOffset, date2: Date | DateTime | DateTimeOffset) => numeric
                    {
                        int expectedArgCount = 2;
                        if (Arguments.Length != expectedArgCount)
                        {
                            throw new QueryException($"Function '{Name}' accepts exactly {expectedArgCount} argument(s).");
                        }

                        // Arguments #1 a Date
                        var arg1 = Arguments[0];
                        if (!(arg1.TryCompile(QxType.Date, ctx, out string date1Sql, out QxNullity date1Nullity) ||
                            arg1.TryCompile(QxType.DateTime, ctx, out date1Sql, out date1Nullity) ||
                            arg1.TryCompile(QxType.DateTimeOffset, ctx, out date1Sql, out date1Nullity)))
                        {
                            throw new QueryException($"Function '{Name}': The first argument {arg1} could not be interpreted as a {QxType.Date}, {QxType.DateTime} or {QxType.DateTimeOffset}.");
                        }

                        var arg2 = Arguments[1];
                        if (!(arg2.TryCompile(QxType.Date, ctx, out string date2Sql, out QxNullity date2Nullity) ||
                        arg2.TryCompile(QxType.DateTime, ctx, out date2Sql, out date2Nullity) ||
                        arg2.TryCompile(QxType.DateTimeOffset, ctx, out date2Sql, out date2Nullity)))
                        {
                            throw new QueryException($"Function '{Name}': The second argument {arg2} could not be interpreted as a {QxType.Date}, {QxType.DateTime} or {QxType.DateTimeOffset}.");
                        }

                        string datePart = nameLower[4..^1]; // Remove "diff" and "s"
                        decimal secondsPerUnit = datePart switch
                        {
                            "day" => 60m * 60m * 24m,
                            "hour" => 60m * 60m,
                            "minute" => 60m,
                            "second" => 1m,
                            _ => throw new Exception()
                        };

                        resultType = QxType.Numeric;
                        resultNullity = date1Nullity | date2Nullity;
                        resultSql = $"(DATEDIFF(SECOND, {date1Sql.DeBracket()}, {date2Sql.DeBracket()}) / {secondsPerUnit:F1})";

                        break;
                    }

                case "not": // (condition: boolean) => boolean                    
                    {
                        int expectedArgCount = 1;
                        if (Arguments.Length != expectedArgCount)
                        {
                            throw new QueryException($"Function '{Name}' accepts exactly {expectedArgCount} argument(s).");
                        }

                        var arg1 = Arguments[0];
                        if (arg1.TryCompile(QxType.Boolean, ctx, out string operandSql, out QxNullity operandNullity))
                        {
                            resultType = QxType.Boolean;
                            resultNullity = operandNullity;
                            if (resultNullity != QxNullity.NotNull)
                            {
                                // Developer mistake
                                throw new InvalidOperationException($"[Bug] A nullable boolean expression: {this}");
                            }

                            if (operandSql == FALSE)
                            {
                                resultSql = TRUE;
                            }
                            else if (operandSql == TRUE)
                            {
                                resultSql = FALSE;
                            }
                            else
                            {
                                resultSql = $"(NOT {operandSql})";
                            }

                            break;
                        }
                        else
                        {
                            throw new QueryException($"Function '{Name}': The argument {arg1} could not be interpreted as a {QxType.Boolean}.");
                        }
                    }

                case "abs": // (value: numeric) => numeric
                    {
                        int expectedArgCount = 1;
                        if (Arguments.Length != expectedArgCount)
                        {
                            throw new QueryException($"Function '{Name}' accepts exactly {expectedArgCount} argument(s).");
                        }

                        var arg1 = Arguments[0];
                        if (!arg1.TryCompile(QxType.Numeric, ctx, out string operandSql, out QxNullity operandNullity))
                        {
                            throw new QueryException($"Function '{Name}': The argument {arg1} could not be interpreted as a {QxType.Numeric}.");
                        }

                        resultType = QxType.Numeric;
                        resultNullity = operandNullity;
                        resultSql = $"ABS({operandSql.DeBracket()})"; // -ve sign

                        break;
                    }

                case "if": // (condition: boolean, value_if_true: X, value_if_false: X) => X
                    {
                        var (conditionSql, arg2, arg3) = IfParameters(ctx);

                        // Complie natively
                        var (ifTrueSql, ifTrueType, ifTrueNullity) = arg2.CompileNative(ctx);
                        if (ifTrueType == QxType.Boolean)
                        {
                            throw new QueryException($"Function '{Name}': The second argument {arg2} cannot be a {QxType.Boolean} expression.");
                        }

                        var (ifFalseSql, ifFalseType, ifFalseNullity) = arg3.CompileNative(ctx);
                        if (ifFalseType == QxType.Boolean)
                        {
                            throw new QueryException($"Function '{Name}': The third argument {arg3} cannot be a {QxType.Boolean} expression.");
                        }

                        if ((ifTrueType == ifFalseType) ||
                            (ifTrueType > ifFalseType && arg2.TryCompile(ifFalseType, ctx, out ifTrueSql, out ifTrueNullity)) ||
                            (ifFalseType > ifTrueType && arg3.TryCompile(ifTrueType, ctx, out ifFalseSql, out ifFalseNullity)))
                        {
                            // Calculate result type, SQL and nullity
                            resultType = ifTrueType > ifFalseType ? ifFalseType : ifTrueType;
                            (resultSql, resultNullity) = IfCompile(conditionSql, ifTrueSql, ifTrueNullity, ifFalseSql, ifFalseNullity);
                            break;
                        }
                        else
                        {
                            throw new QueryException($"Function '{Name}' cannot be used on expressions {arg2} and {arg3} because they have incompatible data types.");
                        }
                    }

                case "isnull": // (value: X, fallback_value: X) => X
                    {
                        var (exp, replacement) = IsNullParameters();

                        var (expSql, expType, expNullity) = exp.CompileNative(ctx);
                        if (expType == QxType.Boolean)
                        {
                            throw new QueryException($"Function '{Name}': The first argument {exp} cannot be a {QxType.Boolean} expression.");
                        }

                        var (replacementSql, replacementType, replacementNullity) = replacement.CompileNative(ctx);
                        if (replacementType == QxType.Boolean)
                        {
                            throw new QueryException($"Function '{Name}': The second argument {replacement} cannot be a {QxType.Boolean} expression.");
                        }

                        // Calculate the native type
                        if ((expType == replacementType) ||
                            (expType > replacementType && exp.TryCompile(replacementType, ctx, out expSql, out expNullity)) ||
                            (replacementType > expType && replacement.TryCompile(expType, ctx, out replacementSql, out replacementNullity)))
                        {
                            // Calculate result type, SQL and nullity
                            resultType = expType > replacementType ? replacementType : expType;
                            (resultSql, resultNullity) = IsNullCompile(expSql, expNullity, replacementSql, replacementNullity);
                            break;
                        }
                        else
                        {
                            throw new QueryException($"Function '{Name}' cannot be used on expressions {exp} and {replacement} because they have incompatible data types.");
                        }
                    }

                case "today": // () => Date | DateTime
                    {
                        resultType = QxType.Date;
                        (resultSql, resultNullity) = CompileToday(ctx, resultType);
                        break;
                    }

                case "now": // () => DateTimeOffset
                    {
                        if (Arguments.Length > 0)
                        {
                            throw new QueryException($"Function '{Name}' does not accept any arguments.");
                        }

                        resultType = QxType.DateTimeOffset;
                        resultNullity = QxNullity.NotNull;

                        string varDef = $"N'{ctx.Now:o}'";
                        string varName = ctx.Variables.AddVariable("DATETIMEOFFSET(7)", varDef);
                        resultSql = $"@{varName}";
                        break;
                    }

                case "me": // () => numeric
                    {
                        if (Arguments.Length > 0)
                        {
                            throw new QueryException($"Function '{Name}' does not accept any arguments.");
                        }

                        resultType = QxType.Numeric;
                        if (ctx.UserId != 0)
                        {
                            resultNullity = QxNullity.NotNull;
                            resultSql = ctx.UserId.ToString();
                        }
                        else
                        {
                            resultNullity = QxNullity.Null;
                            resultSql = null; // Handled later
                        }

                        break;
                    }

                default:
                    {
                        throw new QueryException($"Unknown function '{Name}'.");
                    }
            }

            // Return the result (or NULL if that's the only possible value)
            if (resultNullity == QxNullity.Null)
            {
                resultSql = "NULL";
            }

            return (resultSql, resultType, resultNullity);
        }

        public override bool Equals(object exp)
        {
            return exp is QueryexFunction func &&
                StringComparer.OrdinalIgnoreCase.Equals(func.Name, Name) &&
                func.Arguments.Length == Arguments.Length &&
                Enumerable.Range(0, Arguments.Length)
                    .All(i => func.Arguments[i].Equals(Arguments[i]));
        }

        public override int GetHashCode()
        {
            var nameCode = StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
            var argsCode = Arguments
                .Select(arg => arg.GetHashCode())
                .Aggregate(0, (code1, code2) => code1 ^ code2);

            return nameCode ^ argsCode;
        }

        public override QueryexBase Clone(string[] prefix = null) => new QueryexFunction(Name, Arguments.Select(e => e.Clone(prefix)).ToArray());

        #region Helper Functions

        private (QueryexBase exp, string conditionSql) AggregationParameters(QxCompilationContext ctx)
        {
            if (Arguments.Length < 1 || Arguments.Length > 2)
            {
                throw new QueryException($"No overload for function '{Name}' accepts {Arguments.Length} arguments.");
            }

            var arg1 = Arguments[0];

            string conditionSql = null;
            if (Arguments.Length >= 2)
            {
                var arg2 = Arguments[1];
                if (arg2.TryCompile(QxType.Boolean, ctx, out conditionSql, out QxNullity conditionNullity))
                {
                    if (conditionNullity != QxNullity.NotNull)
                    {
                        // Developer mistake
                        throw new InvalidOperationException($"[Bug] nullable boolean expression {this}.");
                    }
                }
                else
                {
                    throw new QueryException($"Function '{Name}': The second argument {arg2} could not be interpreted as a {QxType.Boolean}.");
                }
            }

            return (arg1, conditionSql);
        }

        private string AggregationCompile(string expSql, string conditionSql)
        {
            return conditionSql == null ?
                $"{Name.ToUpper()}({expSql.DeBracket()})" :
                $"{Name.ToUpper()}(IIF({conditionSql.DeBracket()}, {expSql.DeBracket()}, NULL))";
        }

        private (string conditionSql, QueryexBase valueIfTrue, QueryexBase valueIfFalse) IfParameters(QxCompilationContext ctx)
        {
            int expectedArgCount = 3;
            if (Arguments.Length != expectedArgCount)
            {
                throw new QueryException($"Function '{Name}' accepts exactly {expectedArgCount} argument(s).");
            }

            var arg1 = Arguments[0];
            if (!arg1.TryCompile(QxType.Boolean, ctx, out string conditionSql, out QxNullity conditionNullity))
            {
                throw new QueryException($"Function '{Name}': The first argument {arg1} could not be interpreted as a {QxType.Boolean}.");
            }

            if (conditionNullity != QxNullity.NotNull)
            {
                // Developer mistake
                throw new InvalidOperationException($"[Bug] Nullable boolean expression {this}.");
            }

            var arg2 = Arguments[1];
            var arg3 = Arguments[2];

            return (conditionSql, arg2, arg3);
        }

        private static (string sql, QxNullity nullity) IfCompile(string conditionSql, string ifTrueSql, QxNullity ifTrueNullity, string ifFalseSql, QxNullity ifFalseNullity)
        {
            string resultSql;
            QxNullity resultNullity;

            if (conditionSql == TRUE)
            {
                resultNullity = ifTrueNullity;
                resultSql = ifTrueSql;
            }
            else if (conditionSql == FALSE)
            {
                resultNullity = ifFalseNullity;
                resultSql = ifFalseSql;
            }
            else
            {
                if (ifTrueNullity == QxNullity.NotNull && ifFalseNullity == QxNullity.NotNull)
                {
                    resultNullity = QxNullity.NotNull;
                }
                else if (ifTrueNullity == QxNullity.Null && ifFalseNullity == QxNullity.Null)
                {
                    resultNullity = QxNullity.Null;
                }
                else
                {
                    resultNullity = QxNullity.Nullable;
                }

                resultSql = $"IIF({conditionSql.DeBracket()}, {ifTrueSql.DeBracket()}, {ifFalseSql.DeBracket()})";
            }

            return (resultSql, resultNullity);
        }

        private (QueryexBase exp, QueryexBase replacement) IsNullParameters()
        {
            int expectedArgCount = 2;
            if (Arguments.Length != expectedArgCount)
            {
                throw new QueryException($"Function '{Name}' accepts exactly {expectedArgCount} argument(s).");
            }

            var arg1 = Arguments[0];
            var arg2 = Arguments[1];

            return (arg1, arg2);
        }

        private static (string sql, QxNullity nullity) IsNullCompile(string expSql, QxNullity expNullity, string replacementSql, QxNullity replacementNullity)
        {
            string resultSql;
            QxNullity resultNullity;

            resultNullity = expNullity & replacementNullity;
            resultSql = $"ISNULL({expSql.DeBracket()}, {replacementSql.DeBracket()})";

            return (resultSql, resultNullity);
        }

        private (string numberSql, QueryexBase dateExp) AddDatePartParameters(QxCompilationContext ctx, string nameLower)
        {
            int expectedArgCount = 2;
            if (Arguments.Length != expectedArgCount)
            {
                throw new QueryException($"Function '{Name}' accepts exactly {expectedArgCount} argument(s).");
            }

            string datePart = nameLower[3..^1]; // Remove "add" and "s"

            // Argument #1 Number
            var arg1 = Arguments[0];
            if (arg1.TryCompile(QxType.Numeric, ctx, out string numberSql, out QxNullity numberNullity))
            {
                if (numberNullity != QxNullity.NotNull)
                {
                    throw new QueryException($"Function '{Name}': The first argument {arg1} cannot be a nullable expression.");
                }
            }
            else
            {
                throw new QueryException($"Function '{Name}': The first argument {arg1} could not be interpreted as a {QxType.Numeric}.");
            }

            var dateExp = Arguments[1];
            return (numberSql, dateExp);
        }

        private static (string sql, QxNullity nullity) AddDatePartCompile(string nameLower, string numberSql, string dateSql, QxNullity dateNullity)
        {
            string datePart = nameLower[3..^1]; // Remove "add" and "s"
            QxNullity resultNullity = dateNullity;
            string resultSql = $"DATEADD({datePart.ToUpper()}, {numberSql.DeBracket()}, {dateSql.DeBracket()})"; // Use SQL's built in function

            return (resultSql, resultNullity);


            //// Argument #3 Calendar
            //string calendar = Gregorian; // Default
            //if (Arguments.Length >= 3)
            //{
            //    var arg3 = Arguments[2];
            //    if (arg3 is QueryexQuote calendarQuote)
            //    {
            //        calendar = calendarQuote.Value.ToLower();
            //    }
            //    else
            //    {
            //        throw new QueryException($"Function '{Name}': The third argument must be a simple quote like this: '{UmAlQura}'.");
            //    }
            //}

            //if (datePart == "day" || calendar == Gregorian) // DAY is calendar independent
            //{
            //    resultSql = $"DATEADD({datePart.ToUpper()}, {numberSql.DeBracket()}, {dateSql.DeBracket()})"; // Use SQL's built in function
            //}
            //else
            //{
            //    resultSql = calendar switch
            //    {
            //        UmAlQura => $"[wiz].[fn_UmAlQura_DateAdd]('{datePart[0]}', {numberSql.DeBracket()}, {dateSql.DeBracket()})",
            //        Ethiopian => $"[wiz].[fn_Ethiopian_DateAdd]('{datePart[0]}', {numberSql.DeBracket()}, {dateSql.DeBracket()})",

            //        _ => throw new QueryException($"Function '{Name}': The third argument {Arguments[2]} must be one of the supported calendars: '{string.Join("', '", SupportedCalendars.Select(e => e.ToUpper()))}'.")
            //    };
            //}
        }

        private (string sql, QxNullity nullity) CompileToday(QxCompilationContext ctx, QxType type)
        {
            if (Arguments.Length > 0)
            {
                throw new QueryException($"Function '{Name}' does not accept any arguments.");
            }

            string resultSql;
            switch (type)
            {
                case QxType.Date:
                    {
                        string varDef = $"N'{ctx.Today:yyyy-MM-dd}'";
                        string varName = ctx.Variables.AddVariable("DATE", varDef);
                        resultSql = $"@{varName}";
                        break;
                    }
                case QxType.DateTime:
                    {
                        string varDef = $"N'{ctx.Today:yyyy-MM-ddTHH:mm:ss.ff}'";
                        string varName = ctx.Variables.AddVariable("DATETIME2(2)", varDef);
                        resultSql = $"@{varName}";
                        break;
                    }
                default:
                    throw new InvalidOperationException($"Bug: Calling {nameof(CompileToday)} on an invalidtype {type}.");
            }

            return (resultSql, QxNullity.NotNull);
        }

        #endregion

        #region Function Name Validation

        /// <summary>
        /// First character of a function name must be a letter.
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>True if the first character of the function name is valid according to the condition above, false otherwise</returns>
        private static bool ProperFirstChar(string token)
        {
            return !string.IsNullOrEmpty(token) && char.IsLetter(token[0]);
        }

        /// <summary>
        /// All characters of a function name must be letters, numbers, underscores or dots.
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>True if the characters the function name are valid according to the condition above, false otherwise</returns>
        private static bool ProperChars(string token)
        {
            return !string.IsNullOrEmpty(token) &&
                token.All(c => char.IsLetterOrDigit(c) || c == '_');
        }

        /// <summary>
        /// The function name must not be one of the reserved keywords
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>False if the function name is one of the keywords, true otherwise </returns>
        private static bool NotReservedKeyword(string token)
        {
            return token.ToLower() switch
            {
                "null" or "true" or "false" or "asc" or "desc" => false,
                _ => true,
            };
        }

        /// <summary>
        /// Validates the function's name against all the rules: <see cref="ProperFirstChar(string)"/>,
        /// <see cref="ProperChars(string)"/> and <see cref="NotReservedKeyword(string)"/>
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>True if it passes all the validation rules, false otherwise</returns>
        public static bool IsValidFunctionName(string token)
        {
            return ProperFirstChar(token) && ProperChars(token) && NotReservedKeyword(token);
        }

        #endregion
    }

    public class QueryexBinaryOperator : QueryexBase
    {
        public QueryexBinaryOperator(string op, QueryexBase left, QueryexBase right)
        {
            Operator = op ?? throw new ArgumentNullException(nameof(op));
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public string Operator { get; }

        public QueryexBase Left { get; }

        public QueryexBase Right { get; }

        public override string ToString()
        {
            return $"({Left} {Operator} {Right})";
        }

        public override IEnumerable<QueryexBase> Children
        {
            get
            {
                yield return Left;
                yield return Right;
            }
        }

        public override bool TryCompile(QxType targetType, QxCompilationContext ctx, out string resultSql, out QxNullity resultNullity)
        {
            // This here is merely an optimization
            if (Operator == "+")
            {
                if ((targetType == QxType.String && // String concatenation
                    Left.TryCompile(QxType.String, ctx, out string leftSql, out QxNullity leftNullity) &&
                    Right.TryCompile(QxType.String, ctx, out string rightSql, out QxNullity rightNullity)) ||
                    (targetType == QxType.Numeric && // Numeric
                    Left.TryCompile(QxType.Numeric, ctx, out leftSql, out leftNullity) &&
                    Right.TryCompile(QxType.Numeric, ctx, out rightSql, out rightNullity)))
                {
                    resultNullity = leftNullity | rightNullity;
                    resultSql = $"({leftSql} + {rightSql})";
                    return true;
                }
                else
                {
                    // No other types are possible for +
                    resultNullity = default;
                    resultSql = null;
                    return false;
                }
            }

            return base.TryCompile(targetType, ctx, out resultSql, out resultNullity);
        }

        public override (string, QxType, QxNullity) CompileNative(QxCompilationContext ctx)
        {
            // Note: The way the logic is structured assumes that for ALL operators
            // if either operand is NULL, then the result is NULL

            // Convenience variables
            string leftSql;
            string rightSql;
            QxNullity leftNullity;
            QxNullity rightNullity;

            // The result
            string resultSql;
            QxType resultType;
            QxNullity resultNullity;

            string opLower = Operator?.ToLower();
            switch (opLower)
            {
                case "+":
                    // + maye be either addition or string concatenation
                    // The output type is uniquely determined by the input types (target output type doesn't matter)
                    // since there is no implicit cast from numeric to string or vice versa
                    {
                        if (Left.TryCompile(QxType.Numeric, ctx, out leftSql, out leftNullity) &&
                            Right.TryCompile(QxType.Numeric, ctx, out rightSql, out rightNullity))
                        {
                            // Addition
                            resultType = QxType.Numeric;
                        }
                        else if (Left.TryCompile(QxType.String, ctx, out leftSql, out leftNullity) &&
                            Right.TryCompile(QxType.String, ctx, out rightSql, out rightNullity))
                        {
                            // String concatenation
                            resultType = QxType.String;
                        }
                        else
                        {
                            throw new QueryException($"Operator '{opLower}' cannot be used on expressions {Left} and {Right} because they have incompatible data types.");
                        }

                        resultNullity = leftNullity | rightNullity;
                        resultSql = $"({leftSql} + {rightSql})";
                        break;
                    }

                case "-":
                case "*":
                case "/":
                case "%":
                    // These only accept numerics and return a numeric
                    {
                        resultType = QxType.Numeric;

                        string opSql = opLower;

                        if (!Left.TryCompile(QxType.Numeric, ctx, out leftSql, out leftNullity))
                        {
                            throw new QueryException($"Operator '{Operator}': Left operand {Left} could not be interpreted as {QxType.Numeric}.");
                        }

                        if (!Right.TryCompile(QxType.Numeric, ctx, out rightSql, out rightNullity))
                        {
                            throw new QueryException($"Operator '{Operator}': Right operand {Right} could not be interpreted as {QxType.Numeric}.");

                        }
                        resultNullity = leftNullity | rightNullity;
                        resultSql = $"({leftSql} {opSql} {rightSql})";
                        break;

                    }

                case "&&":
                case "and":
                case "||":
                case "or":
                    // These only accept booleans and return a boolean
                    {
                        resultType = QxType.Boolean;

                        string opSql = opLower switch
                        {
                            "&&" => "AND",
                            "||" => "OR",
                            "and" => "AND",
                            "or" => "OR",
                            _ => opLower,
                        };

                        if (Left.TryCompile(QxType.Boolean, ctx, out leftSql, out leftNullity))
                        {
                            if (Right.TryCompile(QxType.Boolean, ctx, out rightSql, out rightNullity))
                            {
                                resultNullity = leftNullity | rightNullity;
                                if (resultNullity != QxNullity.NotNull)
                                {
                                    // Developer mistake
                                    throw new InvalidOperationException($"[Bug] A nullable boolean expression: {this}");
                                }

                                if (opSql == "AND")
                                {
                                    if (leftSql == FALSE || rightSql == FALSE)
                                    {
                                        resultSql = FALSE;
                                    }
                                    else if (leftSql == TRUE && rightSql == TRUE)
                                    {
                                        resultSql = TRUE;
                                    }
                                    else if (leftSql == TRUE)
                                    {
                                        resultSql = rightSql;
                                    }
                                    else if (rightSql == TRUE)
                                    {
                                        resultSql = leftSql;
                                    }
                                    else
                                    {
                                        resultSql = $"({leftSql} {opSql} {rightSql})";
                                    }

                                    break;
                                }
                                else if (opSql == "OR")
                                {
                                    if (leftSql == TRUE || rightSql == TRUE)
                                    {
                                        resultSql = TRUE;
                                    }
                                    else if (leftSql == FALSE && rightSql == FALSE)
                                    {
                                        resultSql = FALSE;
                                    }
                                    else if (leftSql == FALSE)
                                    {
                                        resultSql = rightSql;
                                    }
                                    else if (rightSql == FALSE)
                                    {
                                        resultSql = leftSql;
                                    }
                                    else
                                    {
                                        resultSql = $"({leftSql} {opSql} {rightSql})";
                                    }

                                    break;
                                }
                                else
                                {
                                    // Developer mistake
                                    throw new InvalidOperationException($"Unknown binary logical operator {opSql}.");
                                }
                            }
                            else
                            {
                                throw new QueryException($"Expression {Right} does not have a boolean type, it cannot be used with operator '{Operator}'.");
                            }
                        }
                        else
                        {
                            throw new QueryException($"Expression {Left} does not have a boolean type, it cannot be used with operator '{Operator}'.");
                        }
                    }

                case "<>":
                case "!=":
                case "ne":
                case ">":
                case "gt":
                case ">=":
                case "ge":
                case "<":
                case "lt":
                case "<=":
                case "le":
                case "=":
                case "eq":
                    // These accept any data type (Except boolean) and always spit out a boolean
                    {
                        resultType = QxType.Boolean;

                        // Translate to SQL operator
                        string opSql = opLower switch
                        {
                            "eq" => "=",
                            "ne" => "<>",
                            "gt" => ">",
                            "ge" => ">=",
                            "lt" => "<",
                            "le" => "<=",
                            _ => opLower,
                        };

                        QxType leftType;
                        (leftSql, leftType, leftNullity) = Left.CompileNative(ctx);
                        if (leftType == QxType.Boolean)
                        {
                            throw new QueryException($"Operator '{Operator}': The left operand {Left} cannot be a {QxType.Boolean} expression.");
                        }

                        QxType rightType;
                        (rightSql, rightType, rightNullity) = Right.CompileNative(ctx);
                        if (rightType == QxType.Boolean)
                        {
                            throw new QueryException($"Operator '{Operator}': The right operand {Right} cannot be a {QxType.Boolean} expression.");
                        }

                        if ((leftType == rightType) ||
                            (leftType > rightType && Left.TryCompile(rightType, ctx, out leftSql, out leftNullity)) ||
                            (rightType > leftType && Right.TryCompile(leftType, ctx, out rightSql, out rightNullity)))
                        {
                            // Comparison functions always return a non nullable boolean
                            resultNullity = QxNullity.NotNull;
                            if (opSql == "=")
                            {
                                resultSql = leftNullity switch
                                {
                                    QxNullity.NotNull => rightNullity switch
                                    {
                                        QxNullity.NotNull => $"({leftSql} = {rightSql})",
                                        QxNullity.Nullable => $"({rightSql} IS NOT NULL AND {leftSql} = {rightSql})",
                                        QxNullity.Null => FALSE,
                                        _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                                    },
                                    QxNullity.Nullable => rightNullity switch
                                    {
                                        QxNullity.NotNull => $"({leftSql} IS NOT NULL AND {leftSql} = {rightSql})",
                                        QxNullity.Nullable => $"(({leftSql} IS NOT NULL AND {rightSql} IS NOT NULL AND {leftSql} = {rightSql}) OR ({leftSql} IS NULL AND {rightSql} IS NULL))",
                                        QxNullity.Null => $"({leftSql} IS NULL)",
                                        _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                                    },
                                    QxNullity.Null => rightNullity switch
                                    {
                                        QxNullity.NotNull => FALSE,
                                        QxNullity.Nullable => $"({rightSql} IS NULL)",
                                        QxNullity.Null => TRUE,
                                        _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                                    },
                                    _ => throw new InvalidOperationException($"Unknown nullity {leftNullity}"),
                                };

                                break;
                            }
                            else if (opSql == "!=" || opSql == "<>")
                            {
                                resultSql = leftNullity switch
                                {
                                    QxNullity.NotNull => rightNullity switch
                                    {
                                        QxNullity.NotNull => $"({leftSql} <> {rightSql})",
                                        QxNullity.Nullable => $"({rightSql} IS NULL OR {leftSql} <> {rightSql})",
                                        QxNullity.Null => TRUE,
                                        _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                                    },
                                    QxNullity.Nullable => rightNullity switch
                                    {
                                        QxNullity.NotNull => $"({leftSql} IS NULL OR {leftSql} <> {rightSql})",
                                        QxNullity.Nullable => $"(({leftSql} IS NULL OR {rightSql} IS NULL OR {leftSql} <> {rightSql}) AND ({leftSql} IS NOT NULL OR {rightSql} IS NOT NULL))",
                                        QxNullity.Null => $"({leftSql} IS NOT NULL)",
                                        _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                                    },
                                    QxNullity.Null => rightNullity switch
                                    {
                                        QxNullity.NotNull => TRUE,
                                        QxNullity.Nullable => $"({rightSql} IS NOT NULL)",
                                        QxNullity.Null => FALSE,
                                        _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                                    },
                                    _ => throw new InvalidOperationException($"Unknown nullity {leftNullity}"),
                                };

                                break;
                            }
                            else // Comparison
                            {
                                resultSql = leftNullity switch
                                {
                                    QxNullity.NotNull => rightNullity switch
                                    {
                                        QxNullity.NotNull => $"({leftSql} {opSql} {rightSql})",
                                        QxNullity.Nullable => $"({rightSql} IS NOT NULL AND {leftSql} {opSql} {rightSql})",
                                        QxNullity.Null => FALSE,
                                        _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                                    },
                                    QxNullity.Nullable => rightNullity switch
                                    {
                                        QxNullity.NotNull => $"({leftSql} IS NOT NULL AND {leftSql} {opSql} {rightSql})",
                                        QxNullity.Nullable => $"({leftSql} IS NOT NULL AND {rightSql} IS NOT NULL AND {leftSql} {opSql} {rightSql})",
                                        QxNullity.Null => FALSE,
                                        _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                                    },
                                    QxNullity.Null => FALSE,
                                    _ => throw new InvalidOperationException($"Unknown nullity {leftNullity}"),
                                };

                                break;
                            }

                            // Should never reach this in theory
                            throw new InvalidOperationException($"[Bug] nullability checks for '{opSql}' were not exhaustive.");
                        }
                        else
                        {
                            // No mutual type was found 
                            throw new QueryException($"Operator '{Operator}' cannot be used on expressions {Left} and {Right} because they have incompatible data types.");
                        }
                    }

                case "descof":
                    // Accepts any data type (Except boolean) and always spit out a non-null boolean
                    // With the proviso that the first operand is always a column access and the 2nd operand does not contain any column access
                    {
                        resultType = QxType.Boolean;

                        // Make sure the left side is vanilla column access
                        if (Left is not QueryexColumnAccess columnAccess)
                        {
                            throw new QueryException($"Operator '{Operator}': The left operand {Left} must be a column access like AccountType.Concept.");
                        }

                        // Make sure the right side contains no column access
                        QueryexColumnAccess ca = Right.ColumnAccesses().FirstOrDefault();
                        if (ca == Right)
                        {
                            throw new QueryException($"Operator '{Operator}': The right operand cannot be a column access expression like {ca}.");
                        }
                        else if (ca != null)
                        {
                            throw new QueryException($"Operator '{Operator}': The right operand cannot contain a column access expression like {ca}.");
                        }

                        QxType leftType;
                        QxType rightType;
                        (leftSql, leftType, leftNullity) = Left.CompileNative(ctx);
                        (rightSql, rightType, rightNullity) = Right.CompileNative(ctx);
                        if (rightType == QxType.Boolean)
                        {
                            throw new QueryException($"Operator '{Operator}': The right operand {Right} cannot be a {QxType.Boolean} expression.");
                        }

                        if ((leftType == rightType) ||
                            (leftType > rightType && Left.TryCompile(rightType, ctx, out leftSql, out leftNullity)) ||
                            (rightType > leftType && Right.TryCompile(leftType, ctx, out rightSql, out rightNullity)))
                        {
                            resultNullity = QxNullity.NotNull;

                            // Prepare the operands
                            var join = ctx.Joins[columnAccess.Path];
                            if (join == null)
                            {
                                // Developer mistake
                                throw new InvalidOperationException($"Bug: The path '{this}' was not found in the joinTree.");
                            }

                            // Make sure it's a tree type
                            var nodeDesc = join.EntityDescriptor.Property("Node");
                            if (nodeDesc == null || nodeDesc.Type != typeof(HierarchyId))
                            {
                                throw new QueryException($"Operator '{Operator}' cannot be used on type {join.EntityDescriptor.Name} since it is not a tree type.");
                            }

                            var propName = columnAccess.Property;
                            var propDesc = join.EntityDescriptor.Property(propName);
                            if (propDesc == null)
                            {
                                // To prevent SQL injection
                                throw new QueryException($"Property '{propName}' does not exist on type {join.EntityDescriptor.Name}.");
                            }

                            // Add a variable before the statement to store the Node of the ancestor
                            string treeSource = ctx.Sources(join.EntityDescriptor.Type);
                            string varDef = $"ISNULL((SELECT TOP 1 [Node] FROM {treeSource} As [T] WHERE [T].[{propName}] = {rightSql}), HIERARCHYID::GetRoot())";
                            string varName = ctx.Variables.AddVariable("HIERARCHYID", varDef);

                            // Use the variable name in the query (more efficient)
                            resultSql = $"([{join.Symbol}].[Node].IsDescendantOf(@{varName}) = 1)";
                            break;
                        }
                        else
                        {
                            // No mutual type was found  
                            throw new QueryException($"Operator '{Operator}' cannot be used on expressions {Left} and {Right} because they have incompatible data types.");
                        }
                    }

                case "contains":
                case "startsw":
                case "endsw":
                    {
                        resultType = QxType.Boolean;

                        if (!Left.TryCompile(QxType.String, ctx, out leftSql, out leftNullity))
                        {
                            throw new QueryException($"Operator '{Operator}': Left operand {Left} could not be interpreted as {QxType.String}.");
                        }

                        if (Right is QueryexQuote quote)
                        {
                            // Since it will be used as the 2nd operand of a LIKE, it must be escaped
                            quote.EscapeForLike();
                        }

                        if (!Right.TryCompile(QxType.String, ctx, out rightSql, out rightNullity))
                        {
                            throw new QueryException($"Operator '{Operator}': Right operand {resultType} could not be interpreted as {QxType.String}.");
                        }

                        resultNullity = QxNullity.NotNull;

                        // Process right SQL to make it suitable as the second operand for the LIKE operator
                        string beforePercent = opLower == "contains" || opLower == "endsw" ? "N'%' + " : "";
                        string afterPercent = opLower == "contains" || opLower == "startsw" ? " + N'%'" : "";

                        // Escape the 2nd LIKE operand unless it's a quote then we already escaped it earlier
                        string escapedRightSql = rightSql;
                        if (!(Right is QueryexQuote))
                        {
                            escapedRightSql = $"REPLACE(REPLACE({escapedRightSql}, N'%', N'[%]'), N'_', N'[_]')";
                        }

                        escapedRightSql = $"{beforePercent}{escapedRightSql}{afterPercent}";

                        resultSql = leftNullity switch
                        {
                            QxNullity.NotNull => rightNullity switch
                            {
                                QxNullity.NotNull => $"({leftSql} LIKE {escapedRightSql})",
                                QxNullity.Nullable => $"({rightSql} IS NOT NULL AND {leftSql} LIKE {escapedRightSql})",
                                QxNullity.Null => FALSE,
                                _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                            },
                            QxNullity.Nullable => rightNullity switch
                            {
                                QxNullity.NotNull => $"({leftSql} IS NOT NULL AND {leftSql} LIKE {escapedRightSql})",
                                QxNullity.Nullable => $"({leftSql} IS NOT NULL AND {rightSql} IS NOT NULL AND {leftSql} LIKE {escapedRightSql})",
                                QxNullity.Null => FALSE,
                                _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                            },
                            QxNullity.Null => FALSE,
                            _ => throw new InvalidOperationException($"Unknown nullity {leftNullity}"),
                        };

                        break;
                    }

                default:
                    // Developer mistake
                    throw new InvalidOperationException($"Unknown binary operator {Operator}"); // Future proofing
            }

            // Return the result (or NULL if that's the only possible value)
            if (resultNullity == QxNullity.Null)
            {
                resultSql = "NULL";
            }

            return (resultSql, resultType, resultNullity);
        }

        public override bool Equals(object exp)
        {
            return exp is QueryexBinaryOperator bo
                && StringComparer.OrdinalIgnoreCase.Equals(bo.Operator, Operator)
                && bo.Left.Equals(Left)
                && bo.Right.Equals(Right);
        }

        public override int GetHashCode()
        {
            int opCode = StringComparer.OrdinalIgnoreCase.GetHashCode(Operator);
            return opCode ^ Left.GetHashCode() ^ Right.GetHashCode();
        }

        public override QueryexBase Clone(string[] prefix = null) => new QueryexBinaryOperator(Operator, Left.Clone(prefix), Right.Clone(prefix));
    }

    public class QueryexUnaryOperator : QueryexBase
    {
        public QueryexUnaryOperator(string op, QueryexBase operand)
        {
            Operator = op ?? throw new ArgumentNullException(nameof(op));
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));
        }

        public string Operator { get; }

        public QueryexBase Operand { get; }

        public override string ToString()
        {
            return $"({Operator} {Operand})";
        }

        public override IEnumerable<QueryexBase> Children
        {
            get
            {
                yield return Operand;
            }
        }

        public override (string sql, QxType type, QxNullity nullity) CompileNative(QxCompilationContext ctx)
        {
            // Note: The way the logic is structured assumes that for ALL operators
            // if either operand is NULL, then the result is NULL

            // Convenience variables
            string operandSql;
            QxNullity operandNullity;

            // The result
            string resultSql;
            QxType resultType;
            QxNullity resultNullity;

            string opLower = Operator?.ToLower();
            switch (opLower)
            {
                case "+":
                case "-":
                    // + maye be either addition or string concatenation
                    // The output type is uniquely determined by the input types (context doesn't matter)
                    // since there is no implicit cast from numeric to string or vice versa
                    {
                        if (!Operand.TryCompile(QxType.Numeric, ctx, out operandSql, out operandNullity))
                        {
                            throw new QueryException($"Operator '{Operator}': Operand {Operand} could not be interpreted as {QxType.Numeric}.");
                        }

                        // Addition
                        resultType = QxType.Numeric;
                        resultNullity = operandNullity;

                        if (opLower == "+")
                        {
                            resultSql = operandSql; // +ve sign (does not do anything)
                        }
                        else if (opLower == "-")
                        {
                            resultSql = $"(-{operandSql})"; // -ve sign
                        }
                        else
                        {
                            // Developer mistake
                            throw new InvalidOperationException($"Unknown unary arithmetic operator {opLower}.");
                        }

                        break;
                    }

                case "!":
                case "not":
                    // These only accept booleans and return a boolean
                    {
                        resultType = QxType.Boolean;

                        if (!Operand.TryCompile(QxType.Boolean, ctx, out operandSql, out operandNullity))
                        {
                            throw new QueryException($"Operator '{Operator}': Operand {Operand} could not be interpreted as {QxType.Boolean}.");
                        }

                        resultNullity = operandNullity;
                        if (resultNullity != QxNullity.NotNull)
                        {
                            // Developer mistake
                            throw new InvalidOperationException($"[Bug] A nullable boolean expression: {this}");
                        }

                        if (operandSql == FALSE)
                        {
                            resultSql = TRUE;
                        }
                        else if (operandSql == TRUE)
                        {
                            resultSql = FALSE;
                        }
                        else
                        {
                            resultSql = $"(NOT {operandSql})";
                        }

                        break;
                    }

                default:
                    // Developer mistake
                    throw new InvalidOperationException($"Unknown unary operator {Operator}"); // Future proofing
            }

            // Return the result (or NULL if that's the only possible value)
            if (resultNullity == QxNullity.Null)
            {
                resultSql = "NULL";
            }

            return (resultSql, resultType, resultNullity);

        }

        public override bool Equals(object exp)
        {
            return exp is QueryexUnaryOperator uo
                && StringComparer.OrdinalIgnoreCase.Equals(uo.Operator, Operator)
                && uo.Operand.Equals(Operand);
        }

        public override int GetHashCode()
        {
            int opCode = StringComparer.OrdinalIgnoreCase.GetHashCode(Operator);
            return opCode ^ Operand.GetHashCode();
        }

        public override QueryexBase Clone(string[] prefix = null) => new QueryexUnaryOperator(Operator, Operand.Clone(prefix));
    }

    public class QueryexQuote : QueryexBase
    {
        private bool _escaped = false;

        public QueryexQuote(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Value { get; private set; }

        /// <summary>
        /// Escapes special characters % and _ in <see cref="Value"/> such that it can be used as the 2nd operand for an SQL LIKE operator
        /// </summary>
        public void EscapeForLike()
        {
            if (!_escaped)
            {
                _escaped = true;
                Value = Value.Replace("%", "[%]").Replace("_", "[_]");
            }
        }

        public override string ToString()
        {
            return $"'{Value.Replace("'", "''")}'";
        }

        public override bool TryCompile(QxType targetType, QxCompilationContext ctx, out string resultSql, out QxNullity resultNullity)
        {
            switch (targetType)
            {
                case QxType.Date:
                    if (DateTime.TryParse(Value, out DateTime d))
                    {
                        d = d.Date; // Remove the time component
                        string varDef = $"N'{d:yyyy-MM-dd}'";
                        string varName = ctx.Variables.AddVariable("DATE", varDef);
                        resultSql = $"@{varName}";
                        resultNullity = QxNullity.NotNull;
                        return true;
                    }
                    else
                    {
                        resultSql = null;
                        resultNullity = default;
                        return false;
                    }
                case QxType.DateTime:
                    if (DateTime.TryParse(Value, out DateTime dt))
                    {
                        string varDef = $"N'{dt:yyyy-MM-ddTHH:mm:ss.ff}'";
                        string varName = ctx.Variables.AddVariable("DATETIME2(2)", varDef);
                        resultSql = $"@{varName}";
                        resultNullity = QxNullity.NotNull;
                        return true;
                    }
                    else
                    {
                        resultSql = null;
                        resultNullity = default;
                        return false;
                    }
                case QxType.DateTimeOffset:
                    if (DateTimeOffset.TryParse(Value, out DateTimeOffset dto))
                    {
                        string varDef = $"N'{dto:o}'";
                        string varName = ctx.Variables.AddVariable("DATETIMEOFFSET(7)", varDef);
                        resultSql = $"@{varName}";
                        resultNullity = QxNullity.NotNull;
                        return true;
                    }
                    else
                    {
                        resultSql = null;
                        resultNullity = default;
                        return false;
                    }
            }

            return base.TryCompile(targetType, ctx, out resultSql, out resultNullity);
        }

        public override (string, QxType, QxNullity) CompileNative(QxCompilationContext ctx)
        {
            // Strings must be added in parameters to prevent SQL injection vulnerability
            var parameterName = ctx.Parameters.AddParameter(Value);
            var sql = $"@{parameterName}";

            return (sql, QxType.String, QxNullity.NotNull);
        }

        /// <summary>
        /// Validates the token against all the rules for expression quote literals
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <param name="decimalValue">The parsed the value as a decimal</param>
        /// <returns>True if the token is a valid expression number, false otherwise</returns>
        public static bool IsValidQuote(string token, out string quoteValue)
        {
            bool match = token.Length >= 2 && token.StartsWith('\'') && token.EndsWith('\'');
            if (match)
            {
                quoteValue = token[1..^1];
            }
            else
            {
                quoteValue = null;
            }

            return match;
        }

        public override bool Equals(object exp)
        {
            return exp is QueryexQuote quote && Value == quote.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override QueryexBase Clone(string[] prefix = null) => new QueryexQuote(Value);
    }

    public class QueryexNumber : QueryexBase
    {
        public QueryexNumber(decimal value, int decimals)
        {
            Value = value;
            Decimals = decimals;
        }

        public decimal Value { get; }

        public int Decimals { get; }

        public override string ToString()
        {
            return Value.ToString();
        }

        /// <summary>
        /// Validates the token against all the rules for expression decimal
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <param name="decimalValue">The parsed the value as a decimal</param>
        /// <returns>True if the token is a valid expression number, false otherwise</returns>
        public static bool IsValidNumber(string token, out decimal decimalValue, out int decimals)
        {
            if (char.IsDigit(token[0]) && char.IsDigit(token[^1]) && token.All(c => char.IsDigit(c) || c == '.'))
            {
                decimalValue = decimal.Parse(token);
                var pieces = token.Split('.');
                decimals = pieces.Length <= 1 ? 0 : pieces[^1].Length;

                return true;
            }
            else
            {
                decimalValue = 0;
                decimals = 0;
                return false;
            }
        }

        public override (string, QxType, QxNullity) CompileNative(QxCompilationContext ctx)
        {
            string format = $"F{Decimals}";
            return (Value.ToString(format), QxType.Numeric, QxNullity.NotNull);
        }

        public override bool Equals(object exp)
        {
            return exp is QueryexNumber n && Value == n.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override QueryexBase Clone(string[] prefix = null) => new QueryexNumber(Value, Decimals);
    }

    public class QueryexNull : QueryexBase
    {
        public override string ToString()
        {
            return "null";
        }

        public override (string, QxType, QxNullity) CompileNative(QxCompilationContext ctx)
        {
            return ("NULL", QxType.Null, QxNullity.Null);
        }

        public override QueryexBase Clone(string[] prefix = null) => new QueryexNull();

        public override bool Equals(object obj)
        {
            return obj is QueryexNull;
        }

        public override int GetHashCode()
        {
            return true.GetHashCode(); // Doesn't matter
        }
    }

    public class QueryexBit : QueryexBase
    {
        public QueryexBit(bool value)
        {
            Value = value;
        }

        public bool Value { get; }

        public override string ToString()
        {
            return Value.ToString().ToLower();
        }

        public override (string, QxType, QxNullity) CompileNative(QxCompilationContext ctx)
        {
            string sql = Value ? "1" : "0";
            return (sql, QxType.Bit, QxNullity.NotNull);
        }

        public override bool Equals(object exp)
        {
            return exp is QueryexBit n && Value == n.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override QueryexBase Clone(string[] prefix = null) => new QueryexBit(Value);
    }

    public class QxCompilationContext
    {
        public QxCompilationContext(JoinTrie joins, Func<Type, string> sources, SqlStatementVariables vars, SqlStatementParameters ps, DateTime today, DateTimeOffset now, int userId)
        {
            Joins = joins ?? throw new ArgumentNullException(nameof(joins));
            Sources = sources ?? throw new ArgumentNullException(nameof(sources));
            Variables = vars ?? throw new ArgumentNullException(nameof(vars));
            Parameters = ps ?? throw new ArgumentNullException(nameof(ps));
            Today = today;
            Now = now;
            UserId = userId;
        }

        public JoinTrie Joins { get; }

        public Func<Type, string> Sources { get; }

        public SqlStatementVariables Variables { get; }

        public SqlStatementParameters Parameters { get; }

        public DateTime Today { get; }

        public DateTimeOffset Now { get; }

        public int UserId { get; }
    }

    /// <summary>
    /// The data types used when type checking and compiling a <see cref="QueryexBase"/>.
    /// The types are arranged such that higher precedence has a smaller value.
    /// An expression with a higher precedence <see cref="QxType"/> cannot be compiled to a <see cref="QxType"/> with a lower precedence.
    /// </summary>
    public enum QxType
    {
        Boolean = 1,        // 0000000001 (highest precedence)
        HierarchyId = 2,    // 0000000010
        Geography = 4,      // 0000000100
        DateTimeOffset = 8, // 0000001000
        DateTime = 16,      // 0000010000
        Date = 32,          // 0000100000
        Numeric = 64,       // 0001000000
        Bit = 128,          // 0010000000
        String = 256,       // 0100000000
        Null = 512,         // 1000000000 (lowest precedence)

        /*
         * [Conversion Rules]
         * The following rules of conversions (->) must be true:
         *      - if A -> C AND B -> C THEN A -> B
         *      - if A -> B then QxType.A > QxType.B
         *      
         * Note: X -> Y means that there exists an expression with a native type of X which can also be compiled to type Y
         *      
         * The purpose of these rules is to efficiently determine the native
         * type of something like If(..., E1, E2), if E1 and E2 both have
         * the same native types, we go for it, otherwise we try to compile
         * the one whose type has a lower precedence to the higher precedence type.         * 
         * 
         * [Current Conversions]
         *      - Null -> Any except boolean
         *      - Bit -> Number
         *      - Bit -> Boolean
         *      - String -> Date
         *      - String -> DateTime
         *      - String -> DateTimeOffset
         */
    }

    public enum QxNullity
    {
        // The values are chosen such that the bitwise operators & and | can be useful
        NotNull = 1,    // 001
        Nullable = 3,   // 011
        Null = 7        // 111
    }

    public enum QxDirection
    {
        None = 0, // Default
        Asc = 1,
        Desc = 2
    }
}
