using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tellma.Services.Utilities;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Represents a plain piece of markup code without any double curly brackets {{ }} in it. This code is outputed as is
    /// </summary>
    public class TemplateMarkup : TemplateBase
    {
        /// <summary>
        /// The plain piece of markup code
        /// </summary>
        public string Content { get; set; }

        public override IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            return AsyncUtil.Empty<Path>();
        }

        public override Task GenerateOutput(StringBuilder builder, EvaluationContext ctx, Func<string, string> encodeFunc = null)
        {
            builder.Append(Content);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates a new instance of <see cref="TemplateMarkup"/> with the given content
        /// </summary>
        public static TemplateMarkup Make(string content) => new TemplateMarkup { Content = content };

        public override string ToString()
        {
            return Content;
        }
    }
}
