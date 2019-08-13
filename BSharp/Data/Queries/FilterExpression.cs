using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BSharp.Data.Queries
{
    /// <summary>
    /// Represents a filter argument which is a tree of ANDs (conjunctions), ORs (disjunctions)
    /// and NOTs (negations) with brackets to override the default precedence of these operators.
    /// For example: "(Order/Total gt 1000) and (Customer/Gender eq 'M')"
    /// This class also contains utilities for parsing filter arguments into <see cref="FilterExpression"/>s.
    /// IMPORTANT: there is a high fidality replica of this in TypeScript for the ClientApp , the two must be kept in sync
    /// </summary>
    public abstract class FilterExpression : IEnumerable<FilterAtom>
    {
        /// <summary>
        /// Parses a string representing a filter argument into a <see cref="FilterExpression"/>. 
        /// The filter argument is a tree of ANDs (conjunctions), ORs (disjunctions)
        /// and NOTs (negations) with brackets to override the default precedence of these operators.
        /// For example "(Order/Total gt 1000) and (Customer/Gender eq 'M')"
        /// </summary>
        public static FilterExpression Parse(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return null;
            }

            string preprocessedFilter = Preprocess(filter);
            IEnumerable<string> tokenStream = Tokenize(preprocessedFilter);
            FilterExpression filterExpression = ParseTokenStream(tokenStream);

            return filterExpression;
        }

        /// <summary>
        /// Preprocesses the filer expression: trimming it and removing any repeated spaces
        /// </summary>
        private static string Preprocess(string filter)
        {
            // Ensure no spaces are repeated
            Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
            filter = regex.Replace(filter, " ");

            // Trim
            filter = filter.Trim();

            // return preprocessed filter argument
            return filter;
        }

        /// <summary>
        /// Lexical Analysis: Turns the filter expression into a stream of recognized tokens
        /// </summary>
        private static IEnumerable<string> Tokenize(string preprocessedFilter)
        {
            // All the symbols, we use spaces before and after the operators to tell them apart
            // Special handling for "not(E)" we need to allow for no space before or after it, and at
            // the same time cannot get confused with property names like "Notes"
            List<string> symbols = new List<string>(new string[] { // the order matters

                    // Logical Operators
                    " and ", " or ", "not",
                    
                    // Parentheses
                    "(", ")",
                });

            // For performance: decompose the filter into a char array and use a string builder to accumulate the characters examined so far
            char[] filterArray = preprocessedFilter.ToCharArray();
            bool insideQuotes = false;
            StringBuilder acc = new StringBuilder();            
            int index = 0;

            string MatchingOperator(int i)
            {
                // This basically finds the first symbol that matches the beginning of the current index at filterArray
                var matchingSymbol = symbols.FirstOrDefault(symbol => (filterArray.Length - i) >= symbol.Length && 
                    Enumerable.Range(0, symbol.Length).All(j => char.ToLower(symbol[j]) == char.ToLower(filterArray[i + j])));

                if(matchingSymbol == "not")
                {
                    // The operator "not" requires more elaborate handling, since it may not necessarily be preceded or superseded by a space
                    // but we don't want to confuse it with properties that contain "not" in their name like "Notes"
                    int prevIndex = i - 1;
                    bool precededProperly = prevIndex < 0 || filterArray[prevIndex] == ' ' || filterArray[prevIndex] == '(';
                    int nextIndex = i + matchingSymbol.Length;
                    bool followedProperly = nextIndex >= filterArray.Length || filterArray[nextIndex] == ' ' || filterArray[nextIndex] == '(';

                    return precededProperly && followedProperly ? matchingSymbol : null;
                }
                else
                {
                    // All the other symbols can be precisely matched
                    return matchingSymbol;
                }
            }

            while (index < filterArray.Length)
            {
                bool isSingleQuote = filterArray[index] == '\'';

                if (isSingleQuote)
                {
                    bool followedBySingleQuote = (index + 1) < filterArray.Length && filterArray[index + 1] == '\'';

                    acc.Append(filterArray[index]);
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
                    string matchingSymbol;
                    if (!insideQuotes && (matchingSymbol = MatchingOperator(index)) != null)
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
                        acc.Append(filterArray[index]);
                        index++;
                    }
                }
            }

            if (insideQuotes)
            {
                // Programmer mistake
                throw new InvalidOperationException("Uneven number of single quotation marks in filter query parameter, quotation marks in literals should be escaped by specifying them twice");
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
        /// Constructs and returns an abstract expression tree (<see cref="FilterExpression"/>) based on a token stream
        /// </summary>
        public static FilterExpression ParseTokenStream(IEnumerable<string> tokens)
        {
            // This is an implementation of the shunting-yard algorithm from Edsger Dijkstra https://bit.ly/1fEvvLI

            var ops = new Stack<string>();
            var output = new Stack<FilterExpression>();

            // Inline function to make it easy to add tokens to the output stack
            void AddToOutput(string token)
            {
                switch (token)
                {
                    case "and":
                        if (output.Count < 2)
                        {
                            throw new InvalidOperationException("Badly formatted filter parameter, a conjunction 'and' was missing one or both of its 2 operands");
                        }

                        output.Push(FilterConjunction.Make(left: output.Pop(), right: output.Pop()));
                        break;
                    case "or":
                        if (output.Count < 2)
                        {
                            throw new InvalidOperationException("Badly formatted filter parameter, a disjunction 'or' was missing one or both of its 2 operands");
                        }

                        output.Push(FilterDisjunction.Make(left: output.Pop(), right: output.Pop()));
                        break;
                    case "not":
                        if (output.Count < 1)
                        {
                            throw new InvalidOperationException("Badly formatted filter parameter, a negation 'not' was missing its operand");
                        }

                        output.Push(FilterNegation.Make(inner: output.Pop()));
                        break;
                    default:
                        output.Push(FilterAtom.Parse(token));
                        break;
                }
            }

            // The shunting-yard implementation
            foreach (var token in tokens)
            {
                if (token == "not")
                {
                    // if it is a logical negation push it on the operators stack
                    ops.Push(token);
                }
                else if (_operators.TryGetValue(token, out OperatorInfo opInfo)) // if it is an operator
                {
                    // inline predicate determines how many items do we pop from the operator stack
                    bool KeepPopping()
                    {
                        /* Modified from Wikipedia: Keep popping while...
                          
                            (the operator at the top of the operator stack is not a left parenthesis) AND 
                            (   
                                (there is a 'not' at the top of the operator stack) OR
                                (there is an operator at the top of the operator stack with greater precedence) OR
                                (the operator at the top of the operator stack has equal precedence and is left associative)
                            )
                         */

                        if (ops.Count == 0)
                        {
                            // There is nothing left to pop
                            return false;
                        }

                        string peek = ops.Peek();
                        _operators.TryGetValue(peek, out OperatorInfo peekInfo);

                        return peek != "(" &&
                            (
                                peek == "not" || peekInfo.Precedence > opInfo.Precedence ||
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
                    ops.Push(token);
                }
                else if (token == ")")
                {
                    if (ops.Count == 0)
                    {
                        // There should have been a left paren in the stack
                        throw new InvalidOperationException("Filter expression contains mismatched parentheses");
                    }

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
                    }
                    else
                    {
                        // There should have been a left paren in the stack
                        throw new InvalidOperationException("Filter expression contains mismatched parentheses");
                    }
                }
                else
                {
                    // It's a simple atom, add it the output
                    AddToOutput(token);
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
                    // Depends whether you want to be forgiving of left parentheses that weren't closed
                    // ops.Pop();

                    // There should not be a left paren in the stack
                    throw new InvalidOperationException("Filter expression contains mismatched parentheses");
                }
            }

            // If the filter expression is valid, there should be exactly one item in the output stack at this stage
            if (output.Count != 1)
            {
                throw new InvalidOperationException("Badly formatted filter parameter");
            }

            return output.Pop();
        }

        /// <summary>
        /// Returns all the <see cref="FilterAtom"/> in this current expression tree
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<FilterAtom> Atoms();

        /// <summary>
        /// Implementation of <see cref="IEnumerable{T}"/>
        /// </summary>
        public IEnumerator<FilterAtom> GetEnumerator()
        {
            return Atoms().GetEnumerator();
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable{T}"/>
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Atoms().GetEnumerator();
        }

        /// <summary>
        /// This list contains the precedence and associativity of supported binary operators, the precedences used are the same as T-SQL (https://bit.ly/2YnyfbV)
        /// </summary>
        private static readonly Dictionary<string, OperatorInfo> _operators = new Dictionary<string, OperatorInfo>
        {
            ["and"] = new OperatorInfo { Precedence = 6, Associativity = Associativity.Left },
            ["or"] = new OperatorInfo { Precedence = 7, Associativity = Associativity.Left },
        };

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
    }
}
