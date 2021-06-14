using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Represents a list of template components that each may contain sub components of their own.
    /// A markup template text is always parsed into a <see cref="TemplateTree"/>
    /// </summary>
    public class TemplateTree : TemplateBase
    {
        /// <summary>
        /// The list of components making up this <see cref="TemplateTree"/>
        /// </summary>
        public List<TemplateBase> Contents { get; private set; } = new List<TemplateBase>();

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
        /// of preprocessing then lexical analysis followed by a syntactic analysis
        /// </summary>
        /// <param name="template">The template string to parse</param>
        /// <returns>A <see cref="TemplateTree"/> which is the abstract representation of the given string</returns>
        public static TemplateTree Parse(string template)
        {
            string preprocessedTemplate = Preprocess(template);
            if (preprocessedTemplate == null)
            {
                return null;
            }

            IEnumerable<string> tokenStream = Tokenize(preprocessedTemplate);
            TemplateTree result = ParseTokenStream(tokenStream);

            return result;
        }

        private static string Preprocess(string template)
        {
            if (template == null)
            {
                return null;
            }

            // return preprocessed filter argument
            return template;
        }

        private static IEnumerable<string> Tokenize(string preprocessedTemplate)
        {
            var templateArray = preprocessedTemplate.ToCharArray();
            bool insideQuotes = false;
            bool insideCurlies = false;
            StringBuilder acc = new StringBuilder();
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

        private static TemplateTree ParseTokenStream(IEnumerable<string> tokenStream)
        {
            // Structure expressions (like *if and *foreach) cause the state to be pushed in this stack
            var stack = new Stack<(StructureBase, TemplateTree)>();

            // State
            var currentTemplate = new TemplateTree();
            var currentStruct = default(StructureBase);

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
                    if (tokenTrim.StartsWith("*")) // Inside the curlies, starts with an asterisk *: Structure component
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
                    else // Inside the curlies, does not start with an asterisk *: An expression
                    {
                        currentTemplate.Contents.Add(TemplexBase.Parse(token));
                    }
                }
                else // Outside the curlies: Plain markup
                {
                    currentTemplate.Contents.Add(TemplateMarkup.Make(token));
                }
            }

            // All scopes that weren't explicitly closed, do so implicitly at the end of the template
            // Keep popping until you're back at the root
            while (stack.Count > 0)
            {
                (_, currentTemplate) = stack.Pop();
            }

            return currentTemplate;
        }
    }
}
