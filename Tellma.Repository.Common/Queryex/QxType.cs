namespace Tellma.Repository.Common.Queryex
{
    /// <summary>
    /// The data types used when type checking and compiling a <see cref="QueryexBase"/>.
    /// The types are arranged such that higher precedence has a smaller value.
    /// An expression with a higher precedence <see cref="QxType"/> cannot be compiled to a <see cref="QxType"/> with a lower precedence.
    /// </summary>
    public enum QxType
    {
        Boolean = 1,        // 0000000001 (highest precedence)
        HierarchyId = 2,    // 0000000010
        Geography = 4,      // 0000000100
        DateTimeOffset = 8, // 0000001000
        DateTime = 16,      // 0000010000
        Date = 32,          // 0000100000
        Numeric = 64,       // 0001000000
        Bit = 128,          // 0010000000
        String = 256,       // 0100000000
        Null = 512,         // 1000000000 (lowest precedence)

        /*
         * [Conversion Rules]
         * The following rules of conversions (->) must be true:
         *      - if A -> C AND B -> C THEN A -> B
         *      - if A -> B then QxType.A > QxType.B
         *      
         * Note: X -> Y means that there exists an expression with a native type of X which can also be compiled to type Y
         *      
         * The purpose of these rules is to efficiently determine the native
         * type of something like If(..., E1, E2), if E1 and E2 both have
         * the same native types, we go for it, otherwise we try to compile
         * the one whose type has a lower precedence to the higher precedence type.         * 
         * 
         * [Current Conversions]
         *      - Null -> Any except boolean
         *      - Bit -> Number
         *      - Bit -> Boolean
         *      - String -> Date
         *      - String -> DateTime
         *      - String -> DateTimeOffset
         */
    }
}
