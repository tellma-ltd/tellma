using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Misc
{
    /// <summary>
    /// Represents a node in the AST that is a basic expression
    /// for example "Name eq 'John'", this node is generated 
    /// when parsing the filter query parameter
    /// </summary>
    public class AstAtom : Ast
    {
        public string Value { get; set; }
    }
}
