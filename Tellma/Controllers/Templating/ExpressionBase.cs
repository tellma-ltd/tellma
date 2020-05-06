using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tellma.Entities;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Base class for all template expressions. Template expressions come inside double curly brackets: {{ expr }},
    /// or as string parameters to some functions: Filter(expr, 'condition_expr'), or in structural components: {{ *if expr }}.
    /// An <see cref="ExpressionBase"/> can be evaluated to a final value given a <see cref="EvaluationContext"/> that supplies
    /// all the referenced functions and variables.
    /// An <see cref="ExpressionBase"/> may contain function invocations that retrieve data from the database.
    /// It can also contain references to variables that are also initialized using database queries. 
    /// Therefore an <see cref="ExpressionBase"/> should also be able to compute statically (before database
    /// variables have been initialized or database functions have been invoked) the list of <see cref="Path"/>s
    /// it needs, that represent the columns to SELECT from the database in order to initialize said variables
    /// and prepare the implementation of said functions, this is implemented in <see cref="TemplateBase.ComputeSelect(EvaluationContext)"/>
    /// <see cref="ExpressionBase"/>s that evaluate to database entities should also statically provide a list of
    /// <see cref="Path"/>s that represent the base path of the returned entity, to aid parent expressions in computing
    /// their SELECT list, this is implemented in <see cref="ComputePaths(EvaluationContext)"/>
    /// </summary>
    public abstract class ExpressionBase : TemplateBase
    {
        #region Symbols & Operators

        /// <summary>
        /// The symbols recognized by the tokenizer
        /// </summary>
        private static readonly List<string> _symbols = new List<string>(new string[] { // the order matters
            
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
                });

        /// <summary>
        /// This list contains the precedence and associativity of supported operators, the precedences used are the same as T-SQL (https://bit.ly/2YnyfbV)
        /// </summary>
        private static readonly Dictionary<string, OperatorInfo> _operators = new Dictionary<string, OperatorInfo>
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

        /// <summary>
        /// If the expression evaluates to a model entity (e.g. a Document), this method should return the base
        /// <see cref="Path"/> of said entity. To aid parent expressions that access this entity in implementing
        /// abstract method <see cref="TemplateBase.ComputeSelect(EvaluationContext)"/>.
        /// The result may be multiple <see cref="Path"/>s in rare cases, for example an IF
        /// function that returns either CreatedBy of document or CreatedBy of Line depending on condition, an
        /// expression invoking this IF function would return two <see cref="Path"/>s
        /// </summary>
        public abstract IAsyncEnumerable<Path> ComputePaths(EvaluationContext ctx);

        /// <summary>
        /// Appends to the <see cref="StringBuilder"/> the output markup evaluated
        /// according to the supplied <see cref="EvaluationContext"/>
        /// </summary>
        public override async Task GenerateOutput(StringBuilder builder, EvaluationContext ctx)
        {
            var value = await Evaluate(ctx);
            builder.Append(ToString(value));
        }

        public string ToString(object value)
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

        /// <summary>
        /// Evaluates the <see cref="ExpressionBase"/> into a final value given a certain <see cref="EvaluationContext"/>.
        /// Sometimes <see cref="Evaluate(EvaluationContext)"/> is invoked before some variables and functions that rely on
        /// database queries have been initialized in order, in which case these variables and functions cannot be referenced
        /// by this <see cref="ExpressionBase"/>.
        /// For example consider Sum(lines, '$.Amount'). The second argument is an expression that must be evaluated
        /// before the variable lines is initialized in order for Sum to determine the SELECT list needed to load those
        /// lines from the database, in this case the list is ['Amount']. Trying something like Sum(lines, doc.Memo) would
        /// throw an exception since doc.Memo cannot be evaluated until data hase been loaded from the database.
        /// </summary>
        public abstract Task<object> Evaluate(EvaluationContext ctx);

        /// <summary>
        /// Parses the string into an <see cref="ExpressionBase"/>
        /// </summary>
        /// <param name="express"></param>
        /// <returns></returns>
        public static ExpressionBase Parse(string express)
        {
            string preprocessedFilter = Preprocess(express);
            if (string.IsNullOrWhiteSpace(preprocessedFilter))
            {
                return null;
            }

            IEnumerable<string> tokenStream = Tokenize(preprocessedFilter);
            ExpressionBase templateExpression = ParseTokenStream(tokenStream);

            return templateExpression;
        }

        private static string Preprocess(string exp)
        {
            if (exp == null)
            {
                return null;
            }

            // Ensure no spaces are repeated
            Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
            exp = regex.Replace(exp, " ");

            // Trim
            exp = exp.Trim();

            // return preprocessed filter argument
            return exp;
        }

        private static IEnumerable<string> Tokenize(string preprocessedExpression)
        {
            // For performance: decompose the filter into a char array and use a string builder to accumulate the characters examined so far
            char[] expArray = preprocessedExpression.ToCharArray();
            bool insideQuotes = false;
            StringBuilder acc = new StringBuilder();
            int index = 0;

            bool TryMatchSymbol(int i, out string symbol)
            {
                // This basically finds the first symbol that matches the beginning of the current index at filterArray
                var matchingSymbol = _symbols.FirstOrDefault(symbol => (expArray.Length - i) >= symbol.Length &&
                    Enumerable.Range(0, symbol.Length).All(j => char.ToLower(symbol[j]) == char.ToLower(expArray[i + j])));

                if (matchingSymbol == ".")
                {
                    // The operator "." requires more elaborate handling, when it is
                    // immediately followed by a digit then it is NOT a separate token
                    int nextIndex = i + matchingSymbol.Length;
                    bool followedByDigit = nextIndex < expArray.Length && char.IsDigit(expArray[nextIndex]);

                    if (followedByDigit) // Decimal point
                    {
                        symbol = null;
                        return false;
                    }
                }

                symbol = matchingSymbol;
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
                        // Add all that has been accumulating before the symbol
                        if (acc.Length > 0)
                        {
                            var token = acc.ToString().Trim();
                            if (!string.IsNullOrWhiteSpace(token))
                            {
                                yield return token;
                            }

                            acc = new StringBuilder();
                        }

                        // And add the symbol  
                        yield return matchingSymbol.ToLower().Trim();
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
                // Programmer mistake
                throw new TemplateException("Uneven number of single quotation marks in filter query parameter, quotation marks in literals should be escaped by specifying them twice");
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

        private static ExpressionBase ParseTokenStream(IEnumerable<string> tokens)
        {
            // This is an implementation of the shunting-yard algorithm from Edsger Dijkstra https://bit.ly/1fEvvLI
            var ops = new Stack<string>();
            var brackets = new Stack<BracketsInfo>();
            var output = new Stack<ExpressionBase>();
            bool lastTokenWasVariable = false;

            // Inline function to make it easy to add tokens to the output stack
            void AddToOutput(string token, bool isFunction = false, int argCount = 0)
            {
                switch (token)
                {
                    case ".":
                        if (output.Count < 2)
                        {
                            throw new TemplateException($"A property accessor '{token}' is missing one or both of its 2 operands");
                        }
                        else
                        {
                            if (!(output.Pop() is ExpressionVariable varCandidate))
                            {
                                throw new TemplateException("The property accessor '.' should be used as follows: <entity_expression>.PropertyName");
                            }

                            var propName = varCandidate.VariableName;
                            var entityCandidate = output.Pop();

                            var exp = ExpressionPropertyAccess.Make(entityCandidate: entityCandidate, propName: propName);
                            output.Push(exp);
                            break;
                        }

                    case "#":
                        if (output.Count < 2)
                        {
                            throw new TemplateException($"An indexer '{token}' is missing one or both of its 2 operands");
                        }
                        else
                        {
                            if (!(output.Pop() is ConstantInteger intCandidate))
                            {
                                throw new TemplateException("The indexer operator '#' should be used as follows: <list_expression>#<number>");
                            }

                            var index = intCandidate.Value;
                            var listCandidate = output.Pop();

                            var exp = ExpressionIndexer.Make(listCandidate: listCandidate, index: index);
                            output.Push(exp);
                            break;
                        }

                    case "*":
                    case "/":
                    case "%":
                    case "+":
                    case "-":
                        if (output.Count < 2)
                        {
                            throw new TemplateException($"An arithmetic operator '{token}' is missing one or both of its 2 operands");
                        }
                        else
                        {
                            var right = output.Pop();
                            var left = output.Pop();
                            output.Push(ExpressionArithmeticOperator.Make(op: token, left: left, right: right));
                            break;
                        }

                    case "=":
                    case "!=":
                    case "<>":
                    case "<=":
                    case "<":
                    case ">=":
                    case ">":
                        if (output.Count < 2)
                        {
                            throw new TemplateException($"A comparison operator '{token}' is missing one or both of its 2 operands");
                        }
                        else
                        {
                            var right = output.Pop();
                            var left = output.Pop();
                            output.Push(ExpressionComparisonOperator.Make(op: token, left: left, right: right));
                            break;
                        }

                    case "!":
                        if (output.Count < 1)
                        {
                            throw new TemplateException($"A negation operator '{token}' is missing its operand");
                        }
                        else
                        {
                            var inner = output.Pop();
                            output.Push(ExpressionNegation.Make(inner: inner));
                            break;
                        }

                    case "&&":
                        if (output.Count < 1)
                        {
                            throw new TemplateException($"A conjunction operator '{token}' is missing one or both of its 2 operands");
                        }
                        else
                        {
                            var right = output.Pop();
                            var left = output.Pop();
                            output.Push(ExpressionConjunction.Make(left: left, right: right));
                            break;
                        }
                    case "||":
                        if (output.Count < 1)
                        {
                            throw new TemplateException($"A disjunction operator '{token}' is missing one or both of its 2 operands");
                        }
                        else
                        {
                            var right = output.Pop();
                            var left = output.Pop();
                            output.Push(ExpressionDisjunction.Make(left: left, right: right));
                            break;
                        }

                    default:
                        if (isFunction)
                        {
                            if (output.Count < argCount)
                            {
                                // Should not happen in theory
                                throw new TemplateException("Bug: Argument count less than output stack size");
                            }

                            List<ExpressionBase> args = new List<ExpressionBase>(argCount);
                            for (int i = 0; i < argCount; i++)
                            {
                                args.Add(output.Pop());
                            }

                            // Reverse the order of items popped from the stack
                            args.Reverse();

                            var exp = ExpressionFunction.Make(functionName: token, args: args.ToArray());
                            output.Push(exp);
                            break;
                        }
                        else
                        {
                            ExpressionBase exp;
                            var tokenLower = token.ToLower();
                            switch (tokenLower)
                            {
                                case "null":
                                    exp = new ConstantNull();
                                    break;

                                case "true":
                                case "false":
                                    exp = new ConstantBoolean { Value = tokenLower == "true", };
                                    break;

                                default:
                                    if (token.StartsWith("'") && token.EndsWith('\''))
                                    {
                                        exp = new ConstantString { Value = token[1..^1] };
                                    }
                                    else if (char.IsDigit(token[0]) && int.TryParse(token, out int intValue)) // <-- This will incorrectly capture decimals
                                    {
                                        exp = new ConstantInteger { Value = intValue };
                                    }
                                    else if (char.IsDigit(token[0]) && char.IsDigit(token[^1]) && token.All(c => (char.IsDigit(c) || c == '.')) && decimal.TryParse(token, out decimal decimalValue))
                                    {
                                        exp = new ConstantDecimal { Value = decimalValue };
                                    }
                                    else if (ExpressionVariable.IsValidVariableName(token))
                                    {
                                        exp = new ExpressionVariable { VariableName = token };
                                    }
                                    else
                                    {
                                        throw new TemplateException($"Unrecognized token: {token}");
                                    }
                                    break;
                            }

                            if (exp is ExpressionVariable)
                            {
                                // In case the very next token was an openning bracket, this VariableExpression is
                                // popped out of the stack and transformed into a InvocationExpression instead
                                lastTokenWasVariable = true;
                            }

                            output.Push(exp);
                            break;
                        }
                }
            }

            foreach (var token in tokens)
            {
                // Copy and reset the lastTokenWasAVariable flag
                var ifLastTokenWasVariable = lastTokenWasVariable;
                lastTokenWasVariable = false;
                bool incrementArgumentCount = false;

                // Shunting-yard implementation
                if (_operators.TryGetValue(token, out OperatorInfo opInfo)) // if it is an operator
                {
                    incrementArgumentCount = true;

                    // inline predicate determines how many items do we pop from the operator stack
                    bool KeepPopping()
                    {
                        /* Modified from Wikipedia: Keep popping while...

                            (the operator at the top of the operator stack is not a left brackets) AND 
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

                        string opsPeek = ops.Peek();
                        bool isOperator = _operators.TryGetValue(opsPeek, out OperatorInfo peekInfo);

                        return opsPeek != "(" &&
                            (
                                // We only push funcs, operators and ('s on the operator stack
                                // so here !isOperator == isFunction
                                !isOperator || peekInfo.Precedence < opInfo.Precedence ||
                                (peekInfo.Precedence == opInfo.Precedence && peekInfo.IsLeftAssociative)
                            );
                    }

                    while (KeepPopping())
                    {
                        AddToOutput(ops.Pop());
                    }

                    ops.Push(token);
                }
                else if (token == "(")
                {
                    // A variable name followed by an open bracket is expected to be a function
                    if (ifLastTokenWasVariable)
                    {
                        var functionName = (output.Pop() as ExpressionVariable).VariableName;
                        ops.Push(functionName);
                    }
                    else
                    {
                        incrementArgumentCount = true;
                    }

                    brackets.Push(new BracketsInfo { IsFunctionInvocation = ifLastTokenWasVariable });
                    ops.Push(token);
                }
                else if (token == ")")
                {
                    // Keep popping from the operator queue until you hit a left paren
                    while (ops.Count > 0 && ops.Peek() != "(")
                    {
                        // Add to output
                        AddToOutput(ops.Pop());
                    }

                    if (ops.Count > 0 && ops.Peek() == "(")
                    {
                        // Pop the left paren
                        ops.Pop();
                        var bracketsInfo = brackets.Pop();
                        if (bracketsInfo.IsFunctionInvocation)
                        {
                            if (bracketsInfo.ArgumentsCount > 0 && bracketsInfo.CommaCount != bracketsInfo.ArgumentsCount - 1)
                            {
                                throw new TemplateException("Blank function arguments are not allowed, use null if you intend to pass a null value");
                            }

                            AddToOutput(ops.Pop(), isFunction: true, argCount: bracketsInfo.ArgumentsCount); // Add the function invocation
                        }
                    }
                    else
                    {
                        // There should have been a left paren in the stack
                        throw new TemplateException("Filter expression contains mismatched brackets");
                    }
                }
                else if (token == ",")
                {
                    if (brackets.Count == 0 || !brackets.Peek().IsFunctionInvocation)
                    {
                        throw new TemplateException("Unexpected comma ',' character. Commas are only used to separate function arguments: Func(arg1, arg2, arg3)");
                    }

                    // Keep popping from the operator queue until you hit the left bracket
                    while (ops.Count > 0 && ops.Peek() != "(")
                    {
                        // Add to output
                        AddToOutput(ops.Pop());
                    }

                    var bracketsInfo = brackets.Peek();
                    bracketsInfo.CommaCount++;
                    if (bracketsInfo.CommaCount > bracketsInfo.ArgumentsCount)
                    {
                        throw new TemplateException("Blank function arguments are not allowed, pass null if that was your intention");
                    }
                }
                else
                {
                    incrementArgumentCount = true;

                    // It's (hopefully) a simple atom => add it the output
                    // IF the very next token is an opening bracket "(" then
                    // this a function invocation, this action is corrected
                    // by popping from the output and pushing in ops
                    AddToOutput(token);
                }

                if (incrementArgumentCount)
                {
                    // Increment the arguments counter if a comma count was incremented earlier
                    if (brackets.Count > 0)
                    {
                        var peek = brackets.Peek();
                        if (peek.IsFunctionInvocation && peek.ArgumentsCount == peek.CommaCount)
                        {
                            peek.ArgumentsCount++;
                        }
                    }
                }
            }

            // Anything left in the operators queue, add it to the output
            while (ops.Count > 0)
            {
                if (ops.Peek() != "(")
                {
                    // Add to output
                    AddToOutput(ops.Pop());
                }
                else
                {
                    // Depends whether you want to be forgiving of left brackets that weren't closed
                    // ops.Pop();

                    // There should not be a left bracket in the stack
                    throw new TemplateException("Filter expression contains mismatched brackets");
                }
            }

            // If the filter expression is valid, there should be exactly one item in the output stack at this stage
            if (output.Count == 0)
            {
                return null;
            }
            else if (output.Count > 1)
            {
                throw new TemplateException("Incorrectly formatted filter parameter");
            }

            return output.Pop();
        }

        #region Helper Types

        private class BracketsInfo
        {
            public bool IsFunctionInvocation { get; set; }
            public int ArgumentsCount { get; set; }
            public int CommaCount { get; set; }
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
    }
}
