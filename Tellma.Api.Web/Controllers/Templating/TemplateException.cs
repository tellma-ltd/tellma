using System;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// All parsing and validation errors from <see cref="TemplateService"/> are thrown in this exception
    /// </summary>
    public class TemplateException : Exception
    {
        public TemplateException(string msg) : base(msg) { }
    }
}
