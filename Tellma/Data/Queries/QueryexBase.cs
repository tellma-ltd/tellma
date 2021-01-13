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

        #region Direction

        public QxDirection Direction { get; set; }
        public bool IsAscending => Direction == QxDirection.Asc;
        public bool IsDescending => Direction == QxDirection.Desc;

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
        /// Returns every <see cref="QueryexColumnAccess"/> within this expression
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<QueryexColumnAccess> ColumnAccesses();

        /// <summary>
        /// True if the current expression is an aggregation function, false otherwise
        /// </summary>
        public virtual bool IsAggregation => false;

        #endregion

        #region Symbols & Operators

        /// <summary>
        /// The symbols recognized by the tokenizer. Note: The order is important, since we take the first match.
        /// So >= should be listed before =
        /// </summary>
        static readonly List<string> _symbols = new List<string>(new string[] { // the order matters
            
                    // Comparison Operators
                    "!=", "<>", "<=", ">=", "<", ">", "=", 

                    // Logical Operators
                    "&&", "||", "!",

                    // Brackets and comma
                    "(", ")", ",",
            
                    // Arithmetic Operators
                    "+", "-", "*", "/", "%",

                    // String Operators (for backward compatibility)
                    " contains ", " startsw ", " endsw ", 
                
                    // Tree Operators (for backward compatibility)
                    " childof ", " descof ",

                    // Logical Operators (for backward compatibility)
                    "not", " and ", " or ",
            
                    // Comparison Operators (for backward compatibility)
                    " gt ", " ge ", " lt ", " le ", " eq ", " ne ",

                    // Directions
                    "asc", "desc",
                });

        /// <summary>
        /// This list contains the precedence and associativity of supported operators (that do not require brackets)
        /// The precedences used are the same as T-SQL (https://bit.ly/2YnyfbV)
        /// </summary>
        private static readonly Dictionary<string, OperatorInfo> _operatorInfos = new Dictionary<string, OperatorInfo>(StringComparer.OrdinalIgnoreCase)
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
            ["childof"] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            ["descof"] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },

            // Logical Operators
            ["!"] = new OperatorInfo { Precedence = 5, Associativity = Associativity.Left },
            ["&&"] = new OperatorInfo { Precedence = 6, Associativity = Associativity.Left },
            ["||"] = new OperatorInfo { Precedence = 7, Associativity = Associativity.Left },
            ["not"] = new OperatorInfo { Precedence = 5, Associativity = Associativity.Left },
            ["and"] = new OperatorInfo { Precedence = 6, Associativity = Associativity.Left },
            ["or"] = new OperatorInfo { Precedence = 7, Associativity = Associativity.Left },
        };

        private static bool ValidUnaryOperator(string op)
        {
            switch (op.ToLower())
            {
                case "-":
                case "+":
                case "!":
                case "not":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns true for operators that can be used 
        /// Note: The function does not check if it's a valid operator in the first place.
        /// </summary>
        private static bool ValidBinaryOperator(string op)
        {
            switch (op.ToLower())
            {
                case "!":
                case "not":
                    return false;
                default:
                    return true;
            }
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

        public static IEnumerable<QueryexBase> Parse(string expressionString, bool expectDirKeywords = false)
        {
            if (string.IsNullOrWhiteSpace(expressionString))
            {
                yield return null;
                yield break;
            }

            IEnumerable<string> tokenStream = Tokenize(expressionString);
            foreach (var expression in ParseTokenStream(tokenStream, expressionString, expectDirKeywords))
            {
                yield return expression;
            }
        }

        private static IEnumerable<string> Tokenize(string expressionString)
        {
            char[] expArray = expressionString.ToCharArray();
            bool insideQuotes = false;
            StringBuilder acc = new StringBuilder();
            int index = 0;

            bool TryMatchSymbol(int i, out string matchingSymbol)
            {
                // This basically finds the first symbol that matches the beginning of the current index at filterArray
                matchingSymbol = _symbols.FirstOrDefault(symbol => (expArray.Length - i) >= symbol.Length &&
                    Enumerable.Range(0, symbol.Length).All(j => symbol[j] == char.ToLower(expArray[i + j])));

                // The operator "not" requires more elaborate handling, since it may not necessarily be preceded or superseded by a space
                // but we don't want to confuse it with properties that contain "not" in their name like "Notes"
                if (matchingSymbol == "not")
                {
                    int prevIndex = i - 1;
                    bool precededProperly = prevIndex < 0 || expArray[prevIndex] == ' ' || expArray[prevIndex] == ',' || expArray[prevIndex] == '(';
                    int nextIndex = i + matchingSymbol.Length;
                    bool followedProperly = nextIndex >= expArray.Length || expArray[nextIndex] == ' ' || expArray[nextIndex] == ',' || expArray[nextIndex] == '(';

                    if (!precededProperly || !followedProperly)
                    {
                        matchingSymbol = null;
                    }
                }

                if (matchingSymbol == "asc" || matchingSymbol == "desc")
                {
                    int prevIndex = i - 1;
                    bool precededProperly = prevIndex < 0 || expArray[prevIndex] == ' ' || expArray[prevIndex] == ',' || expArray[prevIndex] == ')';
                    int nextIndex = i + matchingSymbol.Length;
                    bool followedProperly = nextIndex >= expArray.Length || expArray[nextIndex] == ' ' || expArray[nextIndex] == ',' || expArray[nextIndex] == ')';

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

                        if (matchingSymbol.EndsWith(' '))
                        {
                            index += matchingSymbol.Length - 1; // Gives a chance for a subsequent space-padded operator sumbol to match
                        }
                        else
                        {
                            index += matchingSymbol.Length;
                        }
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
                throw new QueryException($"Uneven number of single quotation marks in ({expressionString}), quotation marks in string literals should be escaped by specifying them twice.");
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

        private static IEnumerable<QueryexBase> ParseTokenStream(IEnumerable<string> tokens, string expressionString, bool expectDirKeywords)
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
            bool currentTokenUsedLikeAPrefix()
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

                var result = output.Pop();

                // Set the direction if any
                if (IsDirectionKeyword(previousToken, out QxDirection dir))
                {
                    result.Direction = dir;
                }

                return result;
            }

            foreach (var currentToken in tokens)
            {
                // Shunting-yard implementation
                previousTokenIsPotentialFunction = currentTokenIsPotentialFunction;
                currentTokenIsPotentialFunction = false;
                expressionTerminated = false;

                bool isDirKeyword = currentToken.Equals("asc", StringComparison.OrdinalIgnoreCase) || currentToken.Equals("desc", StringComparison.OrdinalIgnoreCase);

                if (_operatorInfos.TryGetValue(currentToken, out OperatorInfo opInfo)) // if it is an operator
                {
                    // Increment the argument count if not already incremented
                    IncrementArity();

                    // Determine the operator usage: Unary (like negative sign) vs Binary (like subtraction)
                    bool usedAsUnaryOperator = currentTokenUsedLikeAPrefix();

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
                                    opsPeekInfo.Precedence < opInfo.Precedence ||
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
                            var argCount = bracketsInfo.Arity;

                            // Add the function to the output
                            output.Push(new QueryexFunction(name: functionName, args: bracketsInfo.Arguments.ToArray()));
                        }
                        else if (previousToken == "(")
                        {
                            throw new QueryException("Invalid empty brackets ().");
                        }
                    }
                    else
                    {
                        // There should have been a left paren in the stack
                        throw new QueryException($"Expression contains mismatched brackets");
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
                    currentTokenIsPotentialFunction = currentTokenUsedLikeAPrefix() && QueryexFunction.IsValidFunctionName(currentToken);

                    // It's (hopefully) a simple atom => add it the output
                    // IF this is a valid function name and the very next token is an opening bracket "(" then
                    // this is a function invocation, this action is corrected
                    // by popping from the output and pushing in ops
                    QueryexBase exp;
                    var tokenLower = currentToken.ToLower();
                    switch (tokenLower)
                    {
                        case "null":
                            exp = QueryexNull.Value;
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
                            else if (QueryexNumber.IsValidNumber(currentToken, out decimal decimalValue))
                            {
                                exp = new QueryexNumber(value: decimalValue);
                            }
                            else if (QueryexColumnAccess.IsValidColumnAccess(currentToken, out string[] steps))
                            {
                                exp = new QueryexColumnAccess(steps: steps);
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
        private string[] _path;

        private string _property;

        public QueryexColumnAccess(string[] steps)
        {
            Steps = steps ?? throw new ArgumentNullException(nameof(steps));
        }

        public string[] Steps { get; }

        public string Property => _property ??= Steps.Length > 0 ? Steps[Steps.Length - 1] : null;

        public string[] Path => _path ??= Steps.Length > 0 ? Steps.SkipLast(1).ToArray() : new string[0] { };

        public override string ToString()
        {
            var result = Property;
            if (Path.Length > 0)
            {
                result = string.Join('.', Path) + "." + result;
            }

            return result;
        }

        public override IEnumerable<QueryexColumnAccess> ColumnAccesses()
        {
            yield return this;
        }

        public override (string, QxType, QxNullity) CompileNative(QxCompilationContext ctx)
        {
            // Get the property descriptor
            var join = ctx.Joins[Path];
            if (join == null)
            {
                // Developer mistake
                throw new InvalidOperationException($"Bug: The path '{this}' was not found in the joinTree.");
            }

            var symbol = join.Symbol;
            var propName = Property;
            var propDesc = join.EntityDescriptor.Property(propName);
            if (propDesc == null)
            {
                throw new QueryException($"Property '{propName}' does not exist on type {join.EntityDescriptor.Name}.");
            }

            var propType = Nullable.GetUnderlyingType(propDesc.Type) ?? propDesc.Type;

            // (A) Calculate Nullity
            QxNullity nullity = propDesc.IsNotNull ? QxNullity.NotNull : QxNullity.Nullable;

            // (B) Calculate the type
            QxType type;
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
            var sql = $"[{symbol}].[{propName}]";

            // Return the result
            return (sql, type, nullity);
        }

        #region Column Access Validation

        /// <summary>
        /// First character of a column access must be a letter.
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>True if the first character of the column access is valid according to the condition above, false otherwise</returns>
        private static bool ProperFirstChar(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            var firstChar = token[0];
            return char.IsLetter(firstChar);
        }

        /// <summary>
        /// All characters of a column access must be letters, numbers, underscores or dots.
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>True if the characters the column access are valid according to the condition above, false otherwise</returns>
        private static bool ProperChars(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            return token.All(c => char.IsDigit(c) || char.IsLetter(c) || c == '_' || c == '.');
        }

        /// <summary>
        /// The column access must not be one of the reserved keywords
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>False if the column access is one of the keywords, true otherwise </returns>
        private static bool NotReservedKeyword(string token)
        {
            switch (token.ToLower())
            {
                case "asc":
                case "desc":
                case "null":
                case "true":
                case "false":
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Validates the column access against all the rules: <see cref="ProperFirstChar(string)"/>,
        /// <see cref="ProperChars(string)"/> and <see cref="NotReservedKeyword(string)"/>
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>True if it passes all the validation rules, false otherwise</returns>
        public static bool IsValidColumnAccess(string token, out string[] steps)
        {
            bool match = ProperFirstChar(token) && ProperChars(token) && NotReservedKeyword(token);
            if (match)
            {
                steps = token
                .Split('.')
                .Select(e => e.Trim())
                .Where(e => !string.IsNullOrEmpty(e))
                .ToArray();
            }
            else
            {
                steps = null;
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
        private const string Gregorian = "gr";

        /// <summary>
        /// Ethiopian Calendar
        /// </summary>
        private const string Ethiopian = "et";

        /// <summary>
        /// Umm Al Qura Calendar
        /// </summary>
        private const string UmmAlQura = "uq";

        /// <summary>
        /// All supported calendars
        /// </summary>
        private readonly string[] SupportedCalendars = new string[] { Gregorian, UmmAlQura };

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
            return $"{Name}({string.Join(", ", Arguments.Select(e => e.ToString()))})";
        }

        public override bool IsAggregation => Name?.ToLower() switch
        {
            "sum" => true,
            "count" => true,
            "avg" => true,
            "max" => true,
            "min" => true,
            _ => false,
        };

        public override IEnumerable<QueryexColumnAccess> ColumnAccesses()
        {
            return Arguments.SelectMany(e => e.ColumnAccesses());
        }

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
                        if (nameLower == "max" || nameLower == "min")
                        {
                            (expSql, resultType, resultNullity) = arg1.CompileNative(ctx);
                        }
                        else if (arg1.TryCompile(QxType.Numeric, ctx, out expSql, out resultNullity))
                        {
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
                case "day":
                case "weekday": // (date: Date | DateTime | DateTimeOffset, calendar?: string) => numeric
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
                                    throw new QueryException($"Function '{Name}': The second argument must be a simple quote like this: '{UmmAlQura}'.");
                                }
                            }

                            resultType = QxType.Numeric;
                            resultNullity = dateNullity;

                            if (datePart == "weekday") // The weekday (Sunday, Monday etc..) is calendar independent
                            {
                                resultSql = $"DATEPART({datePart.ToUpper()}, {dateSql.DeBracket()})";
                            }
                            else // All the others depened on calendar
                            {
                                resultSql = calendar switch
                                {
                                    Gregorian => $"DATEPART({datePart.ToUpper()}, {dateSql.DeBracket()})", // Use SQL's built in function
                                    UmmAlQura => $"[wiz].[fn_UmmAlQura_DatePart](N'{datePart}', {dateSql.DeBracket()})",
                                    Ethiopian => $"[wiz].[fn_Ethiopian_DatePart](N'{datePart}', {dateSql.DeBracket()})",

                                    _ => throw new QueryException($"Function '{Name}': The second argument {Arguments[1]} must be one of the supported calendars: '{string.Join("', '", SupportedCalendars.Select(e => e.ToUpper()))}'.")
                                };
                            }

                            break;
                        }
                        else
                        {
                            throw new QueryException($"Function '{Name}': The first argument {arg1} could not be interpreted as a {QxType.Date}, {QxType.DateTime} or {QxType.DateTimeOffset}.");
                        }
                    }

                case "adddays":
                case "addmonths":
                case "addyears": // (date: Date | DateTime | DateTimeOffset, number: numeric) => Date | DateTime | DateTimeOffset
                    {
                        if (Arguments.Length < 2 || Arguments.Length > 3)
                        {
                            throw new QueryException($"No overload for function '{Name}' accepts {Arguments.Length} arguments.");
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

                        // Argument #3 Date
                        var arg2 = Arguments[0];
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

                        // Argument #3 Calendar
                        string calendar = Gregorian; // Default
                        if (Arguments.Length >= 3)
                        {
                            var arg3 = Arguments[2];
                            if (arg3 is QueryexQuote calendarQuote)
                            {
                                calendar = calendarQuote.Value.ToLower();
                            }
                            else
                            {
                                throw new QueryException($"Function '{Name}': The third argument must be a simple quote like this: '{UmmAlQura}'.");
                            }
                        }

                        // Calculate the result
                        resultNullity = dateNullity;
                        if (datePart == "day" || calendar == Gregorian) // DAY is calendar independent
                        {
                            resultSql = $"DATEADD({datePart.ToUpper()}, {numberSql.DeBracket()}, {dateSql.DeBracket()})"; // Use SQL's built in function
                        }
                        else
                        {
                            resultSql = calendar switch
                            {
                                UmmAlQura => $"[wiz].[fn_UmmAlQura_DateAdd](N'{datePart}', {numberSql.DeBracket()}, {dateSql.DeBracket()})",
                                Ethiopian => $"[wiz].[fn_Ethiopian_DateAdd](N'{datePart}', {numberSql.DeBracket()}, {dateSql.DeBracket()})",

                                _ => throw new QueryException($"Function '{Name}': The third argument {Arguments[2]} must be one of the supported calendars: '{string.Join("', '", SupportedCalendars.Select(e => e.ToUpper()))}'.")
                            };
                        }
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
                            throw new QueryException($"Function '{Name}': The first argument {arg1} could not be interpreted as a {QxType.Date}, {QxType.DateTime} or {QxType.DateTimeOffset}.");
                        }
                    }

                case "if":
                    {
                        var (conditionSql, arg2, arg3) = IfParameters(ctx);

                        // Complie natively
                        var (ifTrueSql, ifTrueType, ifTrueNullity) = arg2.CompileNative(ctx);
                        var (ifFalseSql, ifFalseType, ifFalseNullity) = arg3.CompileNative(ctx);

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
                        var (replacementSql, replacementType, replacementNullity) = replacement.CompileNative(ctx);

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

                case "today": // () => date
                    {
                        if (Arguments.Length > 0)
                        {
                            throw new QueryException($"Function '{Name}' does not accept any arguments.");
                        }

                        resultType = QxType.Date;
                        resultNullity = QxNullity.NotNull;
                        resultSql = $"N'{ctx.Today:yyyy-MM-dd}'";
                        break;
                    }

                case "now": // () => datetimeoffset
                    {
                        if (Arguments.Length > 0)
                        {
                            throw new QueryException($"Function '{Name}' does not accept any arguments.");
                        }

                        resultType = QxType.DateTimeOffset;
                        resultNullity = QxNullity.NotNull;
                        resultSql = $"N'{ctx.Now:o}'";
                        break;
                    }

                case "me": // () => numeric
                    {
                        if (Arguments.Length > 0)
                        {
                            throw new QueryException($"Function '{Name}' does not accept any arguments.");
                        }

                        resultType = QxType.Numeric;
                        if (ctx.UserId != null)
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
                        // TODO: Allow injecting custom functions in the context

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

        private (string sql, QxNullity nullity) IfCompile(string conditionSql, string ifTrueSql, QxNullity ifTrueNullity, string ifFalseSql, QxNullity ifFalseNullity)
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

        private (string sql, QxNullity nullity) IsNullCompile(string expSql, QxNullity expNullity, string replacementSql, QxNullity replacementNullity)
        {
            string resultSql;
            QxNullity resultNullity;

            resultNullity = expNullity & replacementNullity;
            resultSql = $"ISNULL({expSql.DeBracket()}, {replacementSql.DeBracket()})";

            return (resultSql, resultNullity);
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
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            var firstChar = token[0];
            return char.IsLetter(firstChar);
        }

        /// <summary>
        /// All characters of a function name must be letters, numbers, underscores or dots.
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>True if the characters the function name are valid according to the condition above, false otherwise</returns>
        private static bool ProperChars(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            return token.All(c => char.IsDigit(c) || char.IsLetter(c) || c == '_');
        }

        /// <summary>
        /// The function name must not be one of the reserved keywords
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>False if the function name is one of the keywords, true otherwise </returns>
        private static bool NotReservedKeyword(string token)
        {
            switch (token.ToLower())
            {
                case "null":
                case "true":
                case "false":
                case "asc":
                case "desc":
                    return false;
                default:
                    return true;
            }
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

        public override IEnumerable<QueryexColumnAccess> ColumnAccesses()
        {
            return Left.ColumnAccesses().Concat(Right.ColumnAccesses());
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

                        if (Left.TryCompile(QxType.Numeric, ctx, out leftSql, out leftNullity))
                        {
                            if (Right.TryCompile(QxType.Numeric, ctx, out rightSql, out rightNullity))
                            {
                                resultNullity = leftNullity | rightNullity;
                                resultSql = $"({leftSql} {opSql} {rightSql})";
                                break;
                            }
                            else
                            {
                                throw new QueryException($"Expression {Right} does not have a numeric type, it cannot be used with operator '{Operator}'.");
                            }
                        }
                        else
                        {
                            throw new QueryException($"Expression {Left} does not have a numeric type, it cannot be used with operator '{Operator}'.");
                        }
                    }

                case "&&":
                case "||":
                case "and":
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
                case ">":
                case ">=":
                case "<":
                case "<=":
                case "=":
                case "!=":
                case "eq":
                case "ne":
                case "gt":
                case "ge":
                case "lt":
                case "le":
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
                        QxType rightType;
                        (leftSql, leftType, leftNullity) = Left.CompileNative(ctx);
                        (rightSql, rightType, rightNullity) = Right.CompileNative(ctx);

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
                        if (!(Left is QueryexColumnAccess columnAccess))
                        {
                            throw new QueryException($"The left operand of {Operator} must be a column access like AccountType.Concept.");
                        }

                        // Make sure the right side contains no column access
                        QueryexColumnAccess ca = Right.ColumnAccesses().FirstOrDefault();
                        if (ca == Right)
                        {
                            throw new QueryException($"The right operand of {Operator} cannot be a column access expression like {ca}.");
                        }
                        else if (ca != null)
                        {
                            throw new QueryException($"The right operand of {Operator} cannot contain a column access expression like {ca}.");
                        }

                        QxType leftType;
                        QxType rightType;
                        (leftSql, leftType, leftNullity) = Left.CompileNative(ctx);
                        (rightSql, rightType, rightNullity) = Right.CompileNative(ctx);

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

                        if (Left.TryCompile(QxType.String, ctx, out leftSql, out leftNullity))
                        {
                            if (Right is QueryexQuote quote)
                            {
                                // Since it will be used as the 2nd operand of a LIKE, it must be escaped
                                quote.EscapeForLike();
                            }

                            if (Right.TryCompile(QxType.String, ctx, out rightSql, out rightNullity))
                            {
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
                            else
                            {
                                throw new QueryException($"Expression {Right} does not have a string type, it cannot be used with operator '{Operator}'.");
                            }
                        }
                        else
                        {
                            throw new QueryException($"Expression {Left} does not have a string type, it cannot be used with operator '{Operator}'.");
                        }
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
            string operand = Operand.ToString();
            if (!operand.StartsWith("("))
            {
                operand = $"({operand})";
            }

            return $"{Operator}{operand}";
        }

        public override IEnumerable<QueryexColumnAccess> ColumnAccesses()
        {
            return Operand.ColumnAccesses();
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
                        if (Operand.TryCompile(QxType.Numeric, ctx, out operandSql, out operandNullity))
                        {
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
                        else
                        {
                            throw new QueryException($"Expression {Operand} does not have a numeric type, it cannot be used with unary operator '{Operator}'.");
                        }
                    }

                case "!":
                case "not":
                    // These only accept booleans and return a boolean
                    {
                        resultType = QxType.Boolean;

                        if (Operand.TryCompile(QxType.Boolean, ctx, out operandSql, out operandNullity))
                        {
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
                            throw new QueryException($"Expression {Operand} does not have a boolean type, it cannot be used with unary operator '{Operator}'.");
                        }
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
            return $"'{Value}'";
        }

        public override bool TryCompile(QxType targetType, QxCompilationContext ctx, out string resultSql, out QxNullity resultNullity)
        {
            switch (targetType)
            {
                case QxType.Date:
                    if (DateTime.TryParse(Value, out DateTime d))
                    {
                        d = d.Date; // Remove the time component
                        resultSql = $"N'{d:yyyy-MM-dd}'";
                        resultNullity = QxNullity.NotNull;
                        return true;
                    }
                    break;
                case QxType.DateTime:
                    if (DateTime.TryParse(Value, out DateTime dt))
                    {
                        resultSql = $"N'{dt:o}'";
                        resultNullity = QxNullity.NotNull;
                        return true;
                    }
                    break;
                case QxType.DateTimeOffset:
                    if (DateTimeOffset.TryParse(Value, out DateTimeOffset dto))
                    {
                        resultSql = $"N'{dto:o}'";
                        resultNullity = QxNullity.NotNull;
                        return true;
                    }
                    break;
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

        public override IEnumerable<QueryexColumnAccess> ColumnAccesses()
        {
            yield break;
        }
    }

    public class QueryexNumber : QueryexBase
    {
        public QueryexNumber(decimal value)
        {
            Value = value;
        }

        public decimal Value { get; }

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
        public static bool IsValidNumber(string token, out decimal decimalValue)
        {
            if (char.IsDigit(token[0]) && char.IsDigit(token[^1]) && token.All(c => char.IsDigit(c) || c == '.') && decimal.TryParse(token, out decimalValue))
            {
                return true;
            }
            else
            {
                decimalValue = 0;
                return false;
            }
        }

        public override IEnumerable<QueryexColumnAccess> ColumnAccesses()
        {
            yield break;
        }

        public override (string, QxType, QxNullity) CompileNative(QxCompilationContext ctx)
        {
            return (Value.ToString(), QxType.Numeric, QxNullity.NotNull);
        }
    }

    public class QueryexNull : QueryexBase
    {
        private static readonly QueryexNull _value = new QueryexNull();

        public static QueryexNull Value => _value;

        private QueryexNull() { }

        public override string ToString()
        {
            return "null";
        }

        public override (string, QxType, QxNullity) CompileNative(QxCompilationContext ctx)
        {
            return ("NULL", QxType.Null, QxNullity.Null);
        }

        public override IEnumerable<QueryexColumnAccess> ColumnAccesses()
        {
            yield break;
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
            return Value.ToString();
        }

        public override (string, QxType, QxNullity) CompileNative(QxCompilationContext ctx)
        {
            string sql = Value ? "1" : "0";
            return (sql, QxType.Bit, QxNullity.NotNull);
        }

        public override IEnumerable<QueryexColumnAccess> ColumnAccesses()
        {
            yield break;
        }
    }

    public class QxCompilationContext
    {
        public QxCompilationContext(JoinTrie joins, Func<Type, string> sources, SqlStatementVariables vars, SqlStatementParameters ps, DateTime today, int? userId)
        {
            Joins = joins ?? throw new ArgumentNullException(nameof(joins));
            Sources = sources ?? throw new ArgumentNullException(nameof(sources));
            Variables = vars ?? throw new ArgumentNullException(nameof(vars));
            Parameters = ps ?? throw new ArgumentNullException(nameof(ps));
            Today = today;
            UserId = userId;
        }

        private DateTimeOffset? _now;

        public JoinTrie Joins { get; }

        public Func<Type, string> Sources { get; }

        public SqlStatementVariables Variables { get; }

        public SqlStatementParameters Parameters { get; }

        public DateTime Today { get; }

        public DateTimeOffset Now => _now ??= DateTimeOffset.UtcNow;

        public int? UserId { get; }
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
