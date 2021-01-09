using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tellma.Entities.Descriptors;

namespace Tellma.Data.Queries
{
    public abstract class Queryex
    {
        #region Abstract Members

        public abstract (QueryexType, QueryexNullability) Type(TypeDescriptor desc);

        public abstract string Compile(QueryexType targetType, EvaluationContext ctx);

        public abstract IEnumerable<QueryexColumnAccess> ColumnAccesses();

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

        #endregion

        #region Parser

        public static IEnumerable<Queryex> Parse(string expressionString)
        {
            if (string.IsNullOrWhiteSpace(expressionString))
            {
                yield return null;
                yield break;
            }

            IEnumerable<string> tokenStream = Tokenize(expressionString);
            foreach (var expression in ParseTokenStream(tokenStream, expressionString))
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

        private static IEnumerable<Queryex> ParseTokenStream(IEnumerable<string> tokens, string expressionString)
        {
            // This is an implementation of the shunting-yard algorithm from Edsger Dijkstra https://bit.ly/1fEvvLI
            var ops = new Stack<(string op, bool isUnary)>();
            var brackets = new Stack<BracketInfo>();
            var output = new Stack<Queryex>();

            // Inline function to pop from the ops stack and apply to the output
            void PopOperatorToOutput()
            {
                var (op, usedAsUnaryOperator) = ops.Pop();

                Queryex exp;
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
                // Some validation
                if (bracketsInfo.Arity > 0 && bracketsInfo.Arguments.Count != bracketsInfo.Arity - 1)
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

            // Inline function called when we hit an expression separating comma or the end of the expression string
            Queryex TerminateCurrentExpression()
            {
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

                return output.Pop();
            }

            // Useful variables
            bool currentTokenIsPotentialFunction = false;
            bool previousTokenIsPotentialFunction;
            string previousToken = null;

            // By inspecting the previous token we can tell if the current token is syntacitcally used
            // like a prefix unary operator or function (as opposed to binary operators)
            bool currentTokenUsedLikeAPrefix()
            {
                return previousToken == null || previousToken == "," || previousToken == "(" || _operatorInfos.ContainsKey(previousToken);
            }

            foreach (var currentToken in tokens)
            {
                // Shunting-yard implementation
                previousTokenIsPotentialFunction = currentTokenIsPotentialFunction;
                currentTokenIsPotentialFunction = false;

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
                else
                {
                    IncrementArity();

                    // Flag it if potential function
                    currentTokenIsPotentialFunction = currentTokenUsedLikeAPrefix() && QueryexFunction.IsValidFunctionName(currentToken);

                    // It's (hopefully) a simple atom => add it the output
                    // IF this is a valid function name and the very next token is an opening bracket "(" then
                    // this is a function invocation, this action is corrected
                    // by popping from the output and pushing in ops
                    Queryex exp;
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
            public List<Queryex> Arguments { get; set; } = new List<Queryex>();
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

    public class QueryexColumnAccess : Queryex
    {
        private string[] _path;

        private string _property;

        public QueryexColumnAccess(string[] steps)
        {
            Steps = steps ?? throw new ArgumentNullException(nameof(steps));
        }

        public string[] Steps { get; }

        public string Property => _property ??= Steps.Length > 0 ? Steps[0] : null;

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

        public override string Compile(QueryexType targetType, EvaluationContext ctx)
        {
            // (A) Prepare the symbol corresponding to the path, e.g. P1
            var join = ctx.Joins[Path];
            if (join == null)
            {
                // Developer mistake
                throw new InvalidOperationException($"Bug: The path '{this}' was not found in the joinTree");
            }
            var symbol = join.Symbol;

            // (B) Determine the type of the property and its value
            var propName = Property;
            var prop = join.EntityDescriptor.Property(propName);
            if (prop == null)
            {
                // Developer mistake
                throw new InvalidOperationException($"Bug: Could not find property {propName} on type {join.EntityDescriptor}");
            }



            return $"[{symbol}].[{propName}]";
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

    public class QueryexFunction : Queryex
    {
        public QueryexFunction(string name, params Queryex[] args)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
            Arguments = args;
        }

        public string Name { get; }

        public Queryex[] Arguments { get; }

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

    public class QueryexBinaryOperator : Queryex
    {
        public QueryexBinaryOperator(string op, Queryex left, Queryex right)
        {
            Operator = op ?? throw new ArgumentNullException(nameof(op));
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public string Operator { get; }

        public Queryex Left { get; }

        public Queryex Right { get; }

        public override string ToString()
        {
            return $"({Left} {Operator} {Right})";
        }

        public override IEnumerable<QueryexColumnAccess> ColumnAccesses()
        {
            return Left.ColumnAccesses().Concat(Right.ColumnAccesses());
        }
    }

    public class QueryexUnaryOperator : Queryex
    {
        public QueryexUnaryOperator(string op, Queryex operand)
        {
            Operator = op ?? throw new ArgumentNullException(nameof(op));
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));
        }

        public string Operator { get; }

        public Queryex Operand { get; }

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
    }

    public class QueryexQuote : Queryex
    {
        public QueryexQuote(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Value { get; }

        public override string ToString()
        {
            return $"'{Value}'";
        }

        public override (QueryexType, QueryexNullability) Type(TypeDescriptor desc)
        {
            return (QueryexType.String, QueryexNullability.NotNull);
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

    public class QueryexNumber : Queryex
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

        public override (QueryexType, QueryexNullability) Type(TypeDescriptor desc)
        {
            return (QueryexType.Numeric, QueryexNullability.NotNull);
        }

        public override string Compile(QueryexType targetType, EvaluationContext ctx)
        {
            return targetType switch
            {
                // QueryexType.String => $"N'{Value}'",
                QueryexType.Numeric => Value.ToString(),
                _ => throw new QueryException($"Could not compile {Value} to an expression of type {targetType}")
            };
        }
    }

    public class QueryexNull : Queryex
    {
        private static readonly QueryexNull _value = new QueryexNull();

        public static QueryexNull Value => _value;

        private QueryexNull() { }

        public override string ToString()
        {
            return "null";
        }

        public override (QueryexType, QueryexNullability) Type(TypeDescriptor desc)
        {
            return (QueryexType.None, QueryexNullability.Null);
        }

        public override string Compile(QueryexType targetType, EvaluationContext ctx)
        {
            return targetType switch
            {
                QueryexType.Boolean => throw new QueryException($"Could not compile null to an expression of type {targetType}"),
                _ => "NULL" // Null can be anything other than boolean
            };
        }

        public override IEnumerable<QueryexColumnAccess> ColumnAccesses()
        {
            yield break;
        }
    }

    public class QueryexBit : Queryex
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

        public override QueryexType Type(TypeDescriptor desc)
        {
            return QueryexType.Bit;
        }

        public override string Compile(QueryexType targetType, EvaluationContext ctx)
        {
            var bit = Value ? "1" : "0";
            return targetType switch
            {
                QueryexType.String => $"N'{bit}'",
                QueryexType.Numeric => bit,
                QueryexType.Bit => bit,
                QueryexType.Boolean => $"{bit} = 1",
                _ => throw new QueryException($"Could not compile {Value.ToString().ToLower()} to an expression of type {targetType}")
            };
        }

        public override IEnumerable<QueryexColumnAccess> ColumnAccesses()
        {
            yield break;
        }
    }

    public class EvaluationContext
    {
        public TypeDescriptor Descriptor { get; }

        public JoinTrie Joins { get; set; }

        public SqlStatementParameters Parameters { get; }

        public DateTime Today { get; }

        public int? UserId { get; }
    }

    [Flags]
    public enum QueryexType
    {
        None = 0,
        String = 1,
        Numeric = 2,
        Date = 4,
        DateTime = 8,
        DateTimeOffset = 16,
        HierarchyId = 32,
        Geography = 64,
        Bit = 128,
        Boolean = 256,

        AnyExceptBoolean = String | Numeric | Date | DateTime | DateTimeOffset | HierarchyId | Geography | Bit,

        ///// <summary>
        ///// <see cref="AnyExceptBoolean"/> but with a flag indicating that other parameters should have the same type
        ///// </summary>
        //X = 1073741824 | AnyExceptBoolean
    }

    public enum QueryexNullability
    {
        NotNull,
        Nullable,
        Null
    }
}
