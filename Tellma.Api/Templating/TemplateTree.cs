using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Represents a list of template components that each may contain sub components of their own.
    /// A markup template text is always parsed into a <see cref="TemplateTree"/>.
    /// </summary>
    public class TemplateTree : TemplateBase
    {
        /// <summary>
        /// The list of components making up this <see cref="TemplateTree"/>.
        /// </summary>
        public List<TemplateBase> Contents { get; private set; } = new();

        public override async IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            foreach (var item in Contents.Where(e => e != null))
            {
                await foreach (var select in item.ComputeSelect(ctx))
                {
                    yield return select;
                }
            }
        }

        public override async Task GenerateOutput(StringBuilder builder, EvaluationContext ctx, Func<string, string> encodeFunc = null)
        {
            foreach (var item in Contents.Where(e => e != null))
            {
                await item.GenerateOutput(builder, ctx, encodeFunc);
            }
        }

        /// <summary>
        /// Parses the given template string into a <see cref="TemplateTree"/> using standard technique
        /// of preprocessing then lexical analysis followed by a syntactic analysis.
        /// </summary>
        /// <param name="template">The template string to parse.</param>
        /// <returns>A <see cref="TemplateTree"/> which is the abstract syntax representation of the given string.</returns>
        public static TemplateTree Parse(string template)
        {
            if (template == null)
            {
                return null;
            }

            // Lexical Analysis
            IEnumerable<string> tokenStream = Tokenize(template);

            // Syntactic Analaysis
            TemplateTree result = ParseTokenStream(tokenStream);

            return result;
        }

        /// <summary>
        /// Lexical analysis: Converts the input string into a stream of recognizable tokens.
        /// The function emits a token for every opening and closing double curly brackets
        /// which are not contained inside quotations and also emits a token for every block of
        /// text between such instances of curly brackets.
        /// <para/>
        /// Example:<br/>
        /// Input "Hello {{ 1 + 2 }} World".<br/>
        /// Output: "Hello", "{{", "1 + 2", "}}", "World".
        /// </summary>
        /// <param name="template">The template string to tokenize.</param>
        /// <returns>A stream of tokens split around double curly brackets.</returns>
        private static IEnumerable<string> Tokenize(string template)
        {
            var templateArray = template.ToCharArray();
            bool insideQuotes = false;
            bool insideCurlies = false;
            var acc = new StringBuilder();
            int index = 0;

            while (index < templateArray.Length)
            {
                if (!insideCurlies)
                {
                    // Outside curlies, we're only looking for opening curlies
                    bool isDoubleOpenCurlies = templateArray[index] == '{' && (index + 1) < templateArray.Length && templateArray[index + 1] == '{';
                    if (isDoubleOpenCurlies)
                    {
                        insideCurlies = true;
                        if (acc.Length > 0)
                        {
                            var token = acc.ToString();
                            if (!string.IsNullOrEmpty(token))
                            {
                                yield return token;
                            }
                        }

                        yield return "{{";

                        acc = new StringBuilder();
                        index += 2;
                    }
                    else
                    {
                        acc.Append(templateArray[index]);
                        index++;
                    }
                }
                else
                {
                    // Inside Curlies, we're looking for closing curlies, ignoring any that appear in the bounds of single quotation marks, e.g. 'Foor }} bar' doesn't count
                    bool isSingleQuote = templateArray[index] == '\'';
                    if (isSingleQuote)
                    {
                        bool followedBySingleQuote = (index + 1) < templateArray.Length && templateArray[index + 1] == '\'';

                        acc.Append(templateArray[index]);
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
                            acc.Append(templateArray[index]);
                            index++;
                        }
                    }
                    else
                    {
                        bool isDoubleClosingCurlies = templateArray[index] == '}' && (index + 1) < templateArray.Length && templateArray[index + 1] == '}';
                        if (!insideQuotes && isDoubleClosingCurlies)
                        {
                            insideCurlies = false;
                            if (acc.Length > 0)
                            {
                                var token = acc.ToString();
                                if (!string.IsNullOrEmpty(token))
                                {
                                    yield return token;
                                }
                            }

                            yield return "}}";

                            acc = new StringBuilder();
                            index += 2;
                        }
                        else
                        {
                            acc.Append(templateArray[index]);
                            index++;
                        }
                    }
                }
            }

            if (insideCurlies)
            {
                // Programmer mistake
                throw new TemplateException("Some opening double curly brackets were not closed");
            }

            if (insideQuotes)
            {
                // Programmer mistake
                throw new TemplateException("Uneven number of single quotation marks in filter query parameter, quotation marks in literals should be escaped by specifying them twice");
            }

            if (acc.Length > 0)
            {
                yield return acc.ToString();
            }
        }

        /// <summary>
        /// Syntactic analysis: Parses a stream of tokens into a <see cref="TemplateTree"/> which is an abstract syntax tree.
        /// This method inspects every incoming token and routes it to one of its subclasses: <see cref="StructureBase"/>,
        /// <see cref="TemplexBase"/> and <see cref="TemplateMarkup"/> to perform a second round of parsing to produce the
        /// final abstract syntax tree.
        /// </summary>
        /// <param name="tokenStream">The token stream to parse.</param>
        /// <returns>The final abstract syntax tree in the form of a <see cref="TemplateTree"/>.</returns>
        private static TemplateTree ParseTokenStream(IEnumerable<string> tokenStream)
        {
            // State
            var currentTemplate = new TemplateTree();
            var currentStruct = default(StructureBase);

            // Structure expressions (like *if and *foreach) cause the state to be pushed in this stack
            var stack = new Stack<(StructureBase, TemplateTree)>();

            bool insideCurlies = false;
            foreach (var token in tokenStream)
            {
                if (token == "{{")
                {
                    insideCurlies = true;
                }
                else if (token == "}}")
                {
                    insideCurlies = false;
                }
                else if (insideCurlies)
                {
                    var tokenTrim = token.Trim();
                    if (tokenTrim.StartsWith("*")) // Inside curlies AND starts with an asterisk * => Structure component
                    {
                        // If it's an *end component, pop the state stack
                        if (tokenTrim.ToLower() == StructureBase._end || tokenTrim.ToLower().StartsWith(StructureBase._end + " "))
                        {
                            if (stack.Count == 0)
                            {
                                throw new TemplateException("Unexpected expression {{" + token + "}}");
                            }

                            // Pop the previous state
                            (currentStruct, currentTemplate) = stack.Pop();
                        }
                        else // If it's anything other then *end, push the state in the stack
                        {
                            // Parse the token and add it to the current template
                            var templateStructure = StructureBase.Parse(tokenTrim);
                            currentTemplate.Contents.Add(templateStructure);

                            // Start of a structural block
                            // Push the current state into the stack
                            stack.Push((currentStruct, currentTemplate));

                            // Start a fresh state
                            currentStruct = templateStructure;
                            currentTemplate = new TemplateTree();
                            currentStruct.Template = currentTemplate;
                        }
                    }
                    else // Inside curlies AND does not start with an asterisk * => A templex expression
                    {
                        currentTemplate.Contents.Add(TemplexBase.Parse(token));
                    }
                }
                else // Outside the curlies: Plain markup text
                {
                    currentTemplate.Contents.Add(TemplateMarkup.Make(token));
                }
            }

            // All scopes that weren't explicitly closed with {{ *end }}, do so implicitly at the end of the template
            // Keep popping until you're back at the root
            while (stack.Count > 0)
            {
                (_, currentTemplate) = stack.Pop();
            }

            return currentTemplate;
        }
    }
}
