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
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public string Content { get; set; }

        public static implicit operator AbstractDataCell(string str)
        {
            return new AbstractDataCell
            {
                Content = str,
                HorizontalAlignment = HorizontalAlignment.Left
            };
        }
    }
}
