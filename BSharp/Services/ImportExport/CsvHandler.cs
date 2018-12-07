using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.ImportExport
{
    public class CsvHandler : FileHandlerBase
    {
        public override AbstractDataFile Parse(byte[] fileBytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Compose(AbstractDataFile abstractFile)
        {
            throw new NotImplementedException();
        }
    }
}
