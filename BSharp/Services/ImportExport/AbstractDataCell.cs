using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.ImportExport
{
    /// <summary>
    /// Represents a single cell in an <see cref="AbstractDataGrid"/>
    /// </summary>
    public class AbstractDataCell
    {
        /// <summary>
        /// Specifies how the numbers need to be formatted, for file formats that support formatting
        /// </summary>
        public string NumberFormat { get; set; }

        /// <summary>
        /// The horizontal alignment
        /// </summary>
        public HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// The content of the cell
        /// </summary>
        public object Content { get; set; }

        public static implicit operator AbstractDataCell(string str)
        {
            return new AbstractDataCell
            {
                Content = str,
                HorizontalAlignment = HorizontalAlignment.Default
            };
        }
    }
}
