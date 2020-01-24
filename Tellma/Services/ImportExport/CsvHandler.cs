using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Services.ImportExport
{
    public class CsvHandler : FileHandlerBase
    {
        private readonly IStringLocalizer _localizer;

        public CsvHandler(IStringLocalizer localizer)
        {
            _localizer = localizer;
        }

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
