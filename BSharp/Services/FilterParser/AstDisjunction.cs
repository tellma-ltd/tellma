namespace BSharp.Services.FilterParser
{
    /// <summary>
    /// Represents a node in the AST that is the logical OR of two AST's
    /// for example "Name eq 'John' or Age gt 18", this node is generated 
    /// when parsing the filter query parameter
    /// </summary>
    internal class AstDisjunction : Ast
    {
        public Ast Left { get; set; }
        public Ast Right { get; set; }
    }
}
