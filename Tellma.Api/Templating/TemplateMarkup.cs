using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tellma.Utilities.Common;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Represents a plain piece of markup without any double curly brackets {{ }} in it. 
    /// This markup is outputed as is.
    /// </summary>
    public class TemplateMarkup : TemplateBase
    {
        /// <summary>
        /// The plain piece of markup code.
        /// </summary>
        public string Content { get; set; }

        public override IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            return AsyncUtil.Empty<Path>();
        }

        public override Task GenerateOutput(StringBuilder builder, EvaluationContext ctx, Func<string, string> encodeFunc = null)
        {
            builder.Append(Content); // Output as is
            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates a new instance of <see cref="TemplateMarkup"/> with the given content.
        /// </summary>
        public static TemplateMarkup Make(string content) => new() { Content = content };

        public override string ToString()
        {
            return Content;
        }
    }
}
