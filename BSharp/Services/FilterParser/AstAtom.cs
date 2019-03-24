namespace BSharp.Services.FilterParser
{
    /// <summary>
    /// Represents a node in the AST that is a basic expression
    /// for example "Name eq 'John'", this node is generated 
    /// when parsing the filter query parameter
    /// </summary>
    internal class AstAtom : Ast
    {
        public string Value { get; set; }
    }
}
