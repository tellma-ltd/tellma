using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tellma.Model.Common;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Base class for all template expressions.
    /// <para/>
    /// Template expressions come inside double curly brackets: {{ expr }},
    /// or as string parameters to some functions: Filter(expr, 'condition_expr'), or in structural components: {{ *if expr }}.<br/>
    /// A <see cref="TemplexBase"/> can be evaluated to a final value given an <see cref="EvaluationContext"/> that supplies
    /// all the referenced functions and variables.<br/>
    /// A <see cref="TemplexBase"/> may contain function invocations that retrieve data from the database.
    /// It can also contain references to variables that are also initialized using database queries. 
    /// Therefore a <see cref="TemplexBase"/> should also be able to compute statically (before database
    /// variables have been initialized or database functions have been invoked) the list of <see cref="Path"/>s
    /// it needs, these <see cref="Path"/>s represent the columns to SELECT from the database in order to initialize
    /// said variables and prepare the implementation of said functions. The computation of those <see cref="Path"/>s 
    /// is implemented in <see cref="TemplateBase.ComputeSelect(EvaluationContext)"/>.<br/>
    /// <see cref="TemplexBase"/>s that evaluate to database entities should also statically provide a list of
    /// <see cref="Path"/>s that represent the base path of the returned entity, to aid parent expressions in computing
    /// their SELECT list, this is implemented in <see cref="ComputePaths(EvaluationContext)"/>.<br/>
    /// <para/>
    /// Note: The prefix "Templex" is short for "Template Expression", in an analogy of "Regex".
    /// </summary>
    public abstract class TemplexBase : TemplateBase
    {
        #region Symbols & Operators

        /// <summary>
        /// The symbols recognized by the tokenizer.
        /// </summary>
        private static readonly List<string> _symbols = new()
        { // the order matters

            // Comparison Operators
            "!=", "<>", "<=", ">=", "<", ">", "=",

            // Logical Operators
            "&&", "||", "!",

            // Brackets and comma
            "(", ")", ",",

            // Property Access and Index Operators
            ".", "#",

            // Arithmetic Operators
            "+", "-", "*", "/", "%"
        };

        /// <summary>
        /// This list contains the precedence and associativity of supported operators, 
        /// the precedences used are the same as T-SQL (https://bit.ly/2YnyfbV).
        /// </summary>
        private static readonly Dictionary<string, OperatorInfo> _operatorInfos = new()
        {
            // Property Access
            ["."] = new OperatorInfo { Precedence = 1, Associativity = Associativity.Left },
            ["#"] = new OperatorInfo { Precedence = 1, Associativity = Associativity.Left },

            // Arithmetic Opreators (take 2 numbers and return a number)
            ["*"] = new OperatorInfo { Precedence = 2, Associativity = Associativity.Left },
            ["/"] = new OperatorInfo { Precedence = 2, Associativity = Associativity.Left },
            ["%"] = new OperatorInfo { Precedence = 2, Associativity = Associativity.Left },
            ["+"] = new OperatorInfo { Precedence = 3, Associativity = Associativity.Left },
            ["-"] = new OperatorInfo { Precedence = 3, Associativity = Associativity.Left },

            // Comparison Operators (take 2 objects and return a boolean)
            ["="] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            ["!="] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            ["<>"] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            ["<="] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            ["<"] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            [">"] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },
            [">="] = new OperatorInfo { Precedence = 4, Associativity = Associativity.Left },

            // Logical Operators (take 1 or 2 booleans and return a boolean)
            ["!"] = new OperatorInfo { Precedence = 5, Associativity = Associativity.Left },
            ["&&"] = new OperatorInfo { Precedence = 6, Associativity = Associativity.Left },
            ["||"] = new OperatorInfo { Precedence = 7, Associativity = Associativity.Left },
        };

        #endregion

        #region Abstract

        /// <summary>
        /// If the expression evaluates to a model entity (e.g. a Document), this method should return the base
        /// <see cref="Path"/> of said entity. To aid parent expressions that access this entity in implementing
        /// abstract method <see cref="TemplateBase.ComputeSelect(EvaluationContext)"/>.<br/>
        /// The result may be multiple <see cref="Path"/>s in rare cases, for example an IF
        /// function that returns either CreatedBy of document or CreatedBy of Line depending on condition, an
        /// expression invoking this IF function would return two <see cref="Path"/>s to account for each possibility.
        /// </summary>
        public abstract IAsyncEnumerable<Path> ComputePaths(EvaluationContext ctx);

        /// <summary>
        /// Evaluates the <see cref="TemplexBase"/> into a final value given a certain <see cref="EvaluationContext"/>.
        /// <para/>
        /// Sometimes <see cref="Evaluate(EvaluationContext)"/> is invoked before some variables and functions that rely on
        /// database queries have been initialized in order to statically compute all the SELECTs that are needed for API calls,
        /// in which case these variables and functions cannot be referenced by this <see cref="TemplexBase"/>.<br/>
        /// For example: consider Sum(lines, '$.Amount'). The second argument is an expression that must be evaluated
        /// before the variable lines is initialized in order for Sum to determine the SELECT list needed to load those
        /// lines from the database, in this case the list is ['Amount']. Trying something like Sum(lines, doc.Memo) would
        /// throw an exception since doc.Memo cannot be evaluated until data hase been loaded from the database.
        /// </summary>
        public abstract Task<object> Evaluate(EvaluationContext ctx);

        /// <summary>
        /// Appends to the <see cref="StringBuilder"/> the output text evaluated
        /// according to the supplied <see cref="EvaluationContext"/>.
        /// </summary>
        public override async Task GenerateOutput(StringBuilder builder, EvaluationContext ctx, Func<string, string> encodeFunc = null)
        {
            encodeFunc ??= s => s; // Encoding for nothing by default

            var value = await Evaluate(ctx);
            var stringValue = ToString(value);
            var encodedStringValue = encodeFunc(stringValue);

            builder.Append(encodedStringValue);
        }

        /// <summary>
        /// Returns the string that represents the object on the resulting template.
        /// </summary>
        public static string ToString(object value)
        {
            if (value is null)
            {
                return "";
            }
            if (value is Entity)
            {
                return $"[{value.GetType().Name}]";
            }
            else if (value is IList)
            {
                var entityType = value.GetType().GetGenericArguments().FirstOrDefault();
                return $"[List of {entityType?.Name ?? "?"}]";
            }
            else
            {
                return value.ToString();
            }
        }

        #endregion

        #region Parser

        /// <summary>
        /// Parses <paramref name="exp"/> into a <see cref="TemplexBase"/>.<br/>
        /// The parser follows the usual steps of: lexical analysis followed by syntacic analysis.
        /// </summary>
        /// <param name="exp">The expression string to parse.</param>
        /// <returns>A <see cref="TemplexBase"/> which is an abstract syntax tree representing the input expression string according to the templex grammer.</returns>
        public static TemplexBase Parse(string expressionString)
        {
            if (string.IsNullOrWhiteSpace(expressionString))
            {
                return null;
            }

            IEnumerable<string> tokenStream = Tokenize(expressionString);
            TemplexBase templateExpression = ParseTokenStream(tokenStream, expressionString);

            return templateExpression;
        }

        /// <summary>
        /// Performs lexical analysis on the input string.
        /// </summary>
        /// <param name="expressionString">The string to perform lexical analysis on.</param>
        /// <returns>A stream of tokens recognizable in the templex grammer.</returns>
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

                if (matchingSymbol == ".")
                {
                    // The operator "." requires more elaborate handling, when it is
                    // immediately followed by a digit then it is NOT a separate token
                    int nextIndex = i + matchingSymbol.Length;
                    bool followedByDigit = nextIndex < expArray.Length && char.IsDigit(expArray[nextIndex]);

                    if (followedByDigit) // Decimal point
                    {
                        matchingSymbol = null;
                    }
                }

                return matchingSymbol != null;
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
                throw new TemplateException($"Uneven number of single quotation marks in {expressionString}, quotation marks in string literals should be escaped by specifying them twice.");
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
        /// Syntactic analysis step or building the abstract syntax tree.
        /// </summary>
        private static TemplexBase ParseTokenStream(IEnumerable<string> tokens, string expressionString)
        {
            // This is an implementation of the shunting-yard algorithm from Edsger Dijkstra https://bit.ly/1fEvvLI
            var ops = new Stack<(string op, bool isUnary)>();
            var brackets = new Stack<BracketInfo>();
            var output = new Stack<TemplexBase>();

            // Inline function to pop from the ops stack and apply to the output
            void PopOperatorToOutput()
            {
                var (op, usedAsUnaryOperator) = ops.Pop();

                TemplexBase exp;
                if (usedAsUnaryOperator)
                {
                    if (!TemplexUnaryOperator.ValidOperator(op))
                    {
                        // "*" OR "* 3"
                        throw new TemplateException($"Infix operator '{op}' is missing its first operand.");
                    }
                    else if (output.Count < 1)
                    {
                        // "!"
                        throw new TemplateException($"Unary operator '{op}' is missing its operand.");
                    }
                    else
                    {
                        var operand = output.Pop();
                        exp = TemplexUnaryOperator.Make(op, operand);
                    }
                }
                else
                {
                    // Binary operator (since we don't have postfix operators)
                    if (!TemplexBinaryOperator.ValidOperator(op))
                    {
                        // "2 ! 3"
                        throw new TemplateException($"Unary Operator '{op}' is used like an infix operator.");
                    }
                    else if (output.Count < 2)
                    {
                        // "3 *"
                        throw new TemplateException($"Infix operator '{op}' is missing its second operand.");
                    }
                    else
                    {
                        var right = output.Pop();
                        var left = output.Pop();

                        switch (op)
                        {
                            case ".":
                                if (right is not TemplexVariable varExpression)
                                {
                                    throw new TemplateException("The property accessor '.' should be used as follows: <entity_expression>.PropertyName.");
                                }

                                string propName = varExpression.VariableName;
                                exp = TemplexPropertyAccess.Make(entityCandidate: left, propName: propName);
                                break;

                            case "#":
                                if (right is not TemplexInteger intExpression)
                                {
                                    throw new TemplateException("The indexer operator '#' should be used as follows: <list_expression>#<number>.");
                                }

                                int index = intExpression.Value;
                                exp = TemplexIndexer.Make(listCandidate: left, index: index);
                                break;

                            default:
                                exp = TemplexBinaryOperator.Make(op, left, right);
                                break;
                        }
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
                        throw new TemplateException("Blank function arguments are not allowed, pass null if that was your intention.");
                    }

                    if (output.Count == 0)
                    {
                        // The previous check should take care of this
                        throw new InvalidOperationException("[Bug] Output stack is empty.");
                    }

                    // Move the argument from the stack to the brackets info, this is in order to parse something like F(1, 2, +) correctly
                    bracketsInfo.Arguments.Add(output.Pop());
                }
            }

            // Useful variables
            bool currentTokenIsPotentialFunction = false;
            bool previousTokenIsPotentialFunction;
            string previousToken = null;

            // By inspecting the previous token we can tell if the current token is syntacitcally used
            // like a prefix unary operator or function (as opposed to binary operators)
            bool CurrentTokenUsedLikeAPrefix()
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
                    ops.Push((currentToken, usedAsUnaryOperator));
                }
                else if (currentToken == "(")
                {
                    if (previousTokenIsPotentialFunction)
                    {
                        // Else the previous token was added to the output incrrectly: remove it
                        output.Pop();
                        ops.Push((previousToken, false)); // Add it as a function to the ops stack instead (isUnary doesn't matter)
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

                            // Add a function to the output
                            var func = TemplexFunction.Make(functionName, args: bracketsInfo.Arguments.ToArray());
                            output.Push(func);
                        }
                        else if (previousToken == "(")
                        {
                            throw new TemplateException("Invalid empty brackets ().");
                        }
                    }
                    else
                    {
                        // There should have been a left paren in the stack
                        throw new TemplateException($"Expression contains mismatched brackets.");
                    }
                }
                else if (currentToken == ",")
                {
                    if (brackets.Count > 0 && brackets.Peek().IsFunction) // A comma in a function invocation
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
                        throw new TemplateException("Unexpected comma ',' character. Commas are only used to separate function arguments: Func(arg1, arg2, arg3).");
                    }
                }
                else
                {
                    IncrementArity();

                    // Flag it if potential function
                    currentTokenIsPotentialFunction = CurrentTokenUsedLikeAPrefix() && TemplexFunction.IsValidFunctionName(currentToken);

                    TemplexBase exp;
                    var tokenLower = currentToken.ToLower();
                    switch (tokenLower)
                    {
                        case "null":
                            exp = new TemplexNull();
                            break;

                        case "true":
                        case "false":
                            exp = new TemplexBoolean { Value = tokenLower == "true", };
                            break;

                        default:
                            if (TemplexQuote.IsValidQuote(currentToken, out string quoteValue))
                            {
                                exp = new TemplexQuote { Value = quoteValue };
                            }
                            else if (TemplexInteger.IsValidInteger(currentToken, out int intValue)) // <-- This will incorrectly capture decimals
                            {
                                exp = new TemplexInteger { Value = intValue };
                            }
                            else if (TemplexDecimal.IsValidDecimal(currentToken, out decimal decimalValue))
                            {
                                exp = new TemplexDecimal { Value = decimalValue };
                            }
                            else if (TemplexVariable.IsValidVariableName(currentToken))
                            {
                                exp = new TemplexVariable { VariableName = currentToken };
                            }
                            else
                            {
                                throw new TemplateException($"Unrecognized token: {currentToken}.");
                            }
                            break;
                    }

                    output.Push(exp);
                }

                previousToken = currentToken;
            }

            ////////////// Final steps

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
                    throw new TemplateException($"Expression contains mismatched brackets: {expressionString}.");
                }
            }

            // If the expression is valid, there should be exactly one item in the output stack at this stage
            if (output.Count == 0)
            {
                return null;
            }
            else if (output.Count > 1)
            {
                throw new TemplateException($"Incorrectly formatted expression: {expressionString}.");
            }

            return output.Pop();
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
            public List<TemplexBase> Arguments { get; set; } = new List<TemplexBase>();

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
