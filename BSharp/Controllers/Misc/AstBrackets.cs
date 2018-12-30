using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Misc
{
    /// <summary>
    /// Represents a node in the AST that is enclosed by a matching pair of brackets
    /// for example "(Name eq 'John')", this node is generated when parsing the filter
    /// query parameter
    /// </summary>
    public class AstBrackets : Ast
    {
        public Ast Inner { get; set; }
    }
}
