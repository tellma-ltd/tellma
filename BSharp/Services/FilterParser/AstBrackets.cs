namespace BSharp.Services.FilterParser
{
    /// <summary>
    /// Represents a node in the AST that is enclosed by a matching pair of brackets
    /// for example "(Name eq 'John')", this node is generated when parsing the filter
    /// query parameter
    /// </summary>
    internal class AstBrackets : Ast
    {
        public Ast Inner { get; set; }
    }
}
