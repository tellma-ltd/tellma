using System;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Represents a single entry in a comma separated expand argument
    /// For example the expand argument "Participant,Lines.Entries" contains two atoms separated by a comma
    /// </summary>
    public class ExpandAtom
    {
        /// <summary>
        /// The path (split along the slashes)
        /// </summary>
        public string[] Path { get; set; }

        /// <summary>
        /// Parses a string representing a single expand path (no commas) into an <see cref="ExpandAtom"/>
        /// </summary>
        /// <param name="atom">String representing a single path (should not contain commas)</param>
        public static ExpandAtom FromExpression(QueryexBase atom)
        {
            // atom comes in the general formats:
            // - "Line.PostingDate"
            // - "Amount"

            if (atom is null)
            {
                throw new ArgumentNullException(nameof(atom));
            }

            if (atom is QueryexColumnAccess columnAccess)
            {
                return new ExpandAtom
                {
                    Path = columnAccess.Steps // Expand steps are expected to terminate with a navigation property
                };
            }
            else
            {
                throw new QueryException($"The expand atom {atom} is not a column access. Only column access literals like (Participant.Lookup1) are permitted in an expand parameter.");
            }
        }
    }
}
