using BSharp.Services.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BSharp.Services.OData
{
    public abstract class FilterExpression : IEnumerable<FilterAtom>
    {
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
        private static IEnumerable<string> Tokenize(string preprocessedFilter)
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
                bool isSingleQuote = preprocessedFilter[index] == '\'';

                if(isSingleQuote)
                {
                    bool followedBySingleQuote = (index + 1) < preprocessedFilter.Length && preprocessedFilter[index + 1] == '\'';

                    acc += preprocessedFilter[index];
                    index++;

                    if (!insideQuotes)
                    {
                        insideQuotes = true;
                    }
                    else if (!followedBySingleQuote)
                    {
                        insideQuotes = false;
                    }
                    else // followed by a single quote
                    {
                        index++; // skip the other single quote, it's just there for escaping the first one
                    }
                }
                else
                {
                    // Everything that is not inside single quotes is ripe for lexical analysis   
                    string matchingSymbol;
                    if(!insideQuotes && (matchingSymbol = symbols.FirstOrDefault(preprocessedFilter.Substring(index).StartsWith)) != null)
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
        public static FilterExpression ParseTokenStream(IEnumerable<string> tokenStream)
        {
            if (tokenStream == null)
            {
                throw new ArgumentNullException(nameof(tokenStream));
            }

            if (tokenStream.IsEnclosedInPairBrackets())
            {
                return FilterBrackets.ParseStream(tokenStream);
            }
            else if (tokenStream.OutsideBrackets().Any(e => e == "or"))
            {
                // OR has lower precedence than AND
                return FilterDisjunction.ParseStream(tokenStream);
            }
            else if (tokenStream.OutsideBrackets().Any(e => e == "and"))
            {
                return FilterConjunction.ParseStream(tokenStream);
            }
            else if (tokenStream.Count() <= 1)
            {
                return FilterAtom.Parse(tokenStream.SingleOrDefault() ?? "");
            }
            else
            {
                // Programmer mistake
                throw new InvalidOperationException("Badly formatted filter parameter");
            }
        }

        public abstract IEnumerable<FilterAtom> Atoms();

        public IEnumerator<FilterAtom> GetEnumerator()
        {
            return Atoms().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Atoms().GetEnumerator();
        }
    }
}
