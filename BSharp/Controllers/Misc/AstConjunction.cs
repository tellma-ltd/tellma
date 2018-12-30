using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Misc
{
    /// <summary>
    /// Represents a node in the AST that is the logical AND of two AST's
    /// for example "Name eq 'John' and Age gt 18", this node is generated 
    /// when parsing the filter query parameter
    /// </summary>
    public class AstConjunction : Ast
    {
        public Ast Left { get; set; }
        public Ast Right { get; set; }
    }
}
