using System.Collections.Generic;
using System.Globalization;

namespace Tellma.Services.ImportExport
{
    /// <summary>
    /// An abstraction of import files, to allow importing multiple file formats, this class represents
    /// a 2-dimensional grid of strings with a few optional metadata, the number of rows is variable, but
    /// the row width is fixed when creating the grid
    /// </summary>
    public class AbstractDataGrid : List<AbstractDataCell[]>
    {
        /// <summary>
        /// Set to true if the file will be read from left to right (When this is supported by the specific format)
        /// </summary>
        public bool IsRightToLeft { get; set; } = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;

        /// <summary>
        /// The fixed size of each row
        /// </summary>
        public int RowSize { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rowSize">The fixed row size</param>
        /// <param name="initialGridCapacity">Improves performance when set correctly, but the grid can grow beyond this value</param>
        public AbstractDataGrid(int rowSize, int initialGridCapacity) : base(initialGridCapacity)
        {
            RowSize = rowSize;
        }

        /// <summary>
        /// Adds a new row to the grid and returns its index
        /// </summary>
        /// <returns></returns>
        public int AddRow()
        {
            Add(new AbstractDataCell[RowSize]);
            return Count - 1;
        }
    }
}
