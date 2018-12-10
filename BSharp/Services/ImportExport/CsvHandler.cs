using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.ImportExport
{
    public class CsvHandler : FileHandlerBase
    {
        public override AbstractDataGrid ToAbstractGrid(Stream fileStream)
        {
            throw new NotImplementedException();
        }

        public override Stream ToFileStream(AbstractDataGrid abstractGrid)
        {
            throw new NotImplementedException();
        }
    }
}
