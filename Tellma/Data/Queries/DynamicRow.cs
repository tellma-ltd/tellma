using System.Collections.Generic;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// When executing aggregate queries the result is dynamic, it may have any number of columns with any datatype,
    /// so a concrete class will not be able to transfer the result.
    /// Instead we transfer the result in a <see cref="DynamicRow"/> which implements IList, and rely on
    /// the index of every property (rather than property name) to map them back to the original query
    /// </summary>
    public class DynamicRow : List<object>
    {
        public DynamicRow() : base()
        {
        }

        public DynamicRow(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// Adds the value at the specified index location (padding the <see cref="DynamicRow"/> with nulls if necessary)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index"></param>
        public void AddAt(object value, int index)
        {
            while (Count < index)
            {
                Add(null);
            }

            if (Count == index)
            {
                Add(value);
            }
            else
            {
                this[index] = value;
            }
        }
    }
}
