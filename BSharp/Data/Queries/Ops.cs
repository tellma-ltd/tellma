namespace BSharp.Data.Queries
{
    /// <summary>
    /// Recognizable binary operators that can be used in filter expressions
    /// </summary>
    public static class Ops
    {
        /// <summary>
        /// Greater than
        /// </summary>
        public const string gt = nameof(gt);

        /// <summary>
        /// Greater than or equals
        /// </summary>
        public const string ge = nameof(ge);

        /// <summary>
        /// Less than
        /// </summary>
        public const string lt = nameof(lt);

        /// <summary>
        /// Less than or equals
        /// </summary>
        public const string le = nameof(le);

        /// <summary>
        /// Equals
        /// </summary>
        public const string eq = nameof(eq);

        /// <summary>
        /// Not equals
        /// </summary>
        public const string ne = nameof(ne);

        /// <summary>
        /// String contains
        /// </summary>
        public const string contains = nameof(contains);

        /// <summary>
        /// String does not contain
        /// </summary>
        public const string ncontains = nameof(ncontains);

        /// <summary>
        /// String starts with
        /// </summary>
        public const string startsw = nameof(startsw);

        /// <summary>
        /// String does not start with
        /// </summary>
        public const string nstartsw = nameof(nstartsw);

        /// <summary>
        /// String ends with
        /// </summary>
        public const string endsw = nameof(endsw);

        /// <summary>
        /// String does not end with
        /// </summary>
        public const string nendsw = nameof(nendsw);

        /// <summary>
        /// Tree entity is child of Id
        /// </summary>
        public const string childof = nameof(childof);

        /// <summary>
        /// Tree entity is descendant of Id
        /// </summary>
        public const string descof = nameof(descof);
    }
}
