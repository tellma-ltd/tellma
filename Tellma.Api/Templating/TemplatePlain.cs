using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tellma.Utilities.Common;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Represents a plain piece of template without any double curly brackets {{ }} in it. 
    /// This text is outputed as is.
    /// </summary>
    public class TemplatePlain : TemplateBase
    {
        /// <summary>
        /// The plain piece of template text.
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
        /// Creates a new instance of <see cref="TemplatePlain"/> with the given content.
        /// </summary>
        public static TemplatePlain Make(string content) => new() { Content = content };

        public override string ToString()
        {
            return Content;
        }
    }
}
