using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.ImportExport
{
    public abstract class FileHandlerBase
    {
        public abstract AbstractDataFile Parse(byte[] fileBytes);
        public abstract byte[] Parse(AbstractDataFile abstractFile);
    }
}
