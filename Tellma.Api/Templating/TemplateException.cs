using System;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// All parsing and validation errors from <see cref="TemplateService"/> are thrown in this exception.
    /// </summary>
    public class TemplateException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateException"/> class.
        /// </summary>
        /// <param name="msg">The message that describes the error.</param>
        public TemplateException(string msg) : base(msg) { }
    }
}
