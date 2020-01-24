using System;
using System.Collections.Generic;
using System.Globalization;

namespace Tellma.Services.ImportExport
{
    public class FileSpecs : List<BasicFieldSpecs>
    {
        public FileSpecs()
        {
            IsRightToLeft = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;
        }

        public string Version { get; set; }
        public bool IsRightToLeft { get; set; }
    }

    public class BasicFieldSpecs
    {
        public string DisplayName { get; set; }
        public Func<object, string> Format { get; set; }
        public Func<string, object> Parse { get; set; }
    }
    
}
