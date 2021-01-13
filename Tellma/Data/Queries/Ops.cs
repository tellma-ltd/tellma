// TODO: Delete
namespace Tellma.Data.Queries
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
        /// Greater than
        /// </summary>
        public const string gtSign = ">";

        /// <summary>
        /// Greater than or equals
        /// </summary>
        public const string ge = nameof(ge);

        /// <summary>
        /// Greater than or equals
        /// </summary>
        public const string geSign = ">=";

        /// <summary>
        /// Less than
        /// </summary>
        public const string lt = nameof(lt);

        /// <summary>
        /// Less than
        /// </summary>
        public const string ltSign = "<";

        /// <summary>
        /// Less than or equals
        /// </summary>
        public const string le = nameof(le);

        /// <summary>
        /// Less than or equals
        /// </summary>
        public const string leSign = "<=";

        /// <summary>
        /// Equals
        /// </summary>
        public const string eq = nameof(eq);

        /// <summary>
        /// Equals
        /// </summary>
        public const string eqSign = "=";

        /// <summary>
        /// Not equals
        /// </summary>
        public const string ne = nameof(ne);

        /// <summary>
        /// Not equals
        /// </summary>
        public const string neSign = "!=";

        /// <summary>
        /// Not equals
        /// </summary>
        public const string neSign2 = "<>";

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
