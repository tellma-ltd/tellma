//using System;
//using System.Linq;

//namespace Tellma.Data.Queries
//{
//    /// <summary>
//    /// Represents a single entry in a comma separated select argument
//    /// For example the select argument "Line.PostingDate,Amount" contains two atoms separated by a comma
//    /// </summary>
//    public class SelectAtom
//    {
//        /// <summary>
//        /// The path component of the atom (split along the dots)
//        /// </summary>
//        public string[] Path { get; set; }

//        /// <summary>
//        /// The property subject of the <see cref="SelectAtom"/>
//        /// </summary>
//        public string Property { get; set; }

//        /// <summary>
//        /// Extracts a <see cref="SelectAtom"/> from a <see cref="QueryexBase"/> object
//        /// </summary>
//        /// <param name="atom">String representing a single atom (should not contain commas)</param>
//        public static SelectAtom FromExpression(QueryexBase atom)
//        {
//            // atom comes in the general formats:
//            // - "Line.PostingDate"
//            // - "Value"

//            if (atom is null)
//            {
//                throw new ArgumentNullException(nameof(atom));
//            }

//            if (atom is QueryexColumnAccess columnAccess)
//            {
//                return new SelectAtom
//                {
//                    Path = columnAccess.Path,
//                    Property = columnAccess.Property
//                };
//            }
//            else
//            {
//                throw new QueryException($"The select atom {atom} is not a column access. Only column access literals like (Line.PostingDate) are permitted in a select parameter.");
//            }
//        }
//    }
//}
