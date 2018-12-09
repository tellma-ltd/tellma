using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.ImportExport
{
    /// <summary>
    /// Every implementations of this class is able to unpack a file of a certain format (e.g. XLSX or CSV)
    /// into an abstract 2D grid of strings which can then be further processed.
    /// </summary>
    public abstract class FileHandlerBase
    {
        public abstract AbstractDataGrid ToAbstractGrid(Stream fileStream);
        public abstract Stream ToFileStream(AbstractDataGrid abstractFile);
    }
}
