using System.Collections.Generic;

namespace Tellma.Api.Dto
{
    public class PrintingPreviewTemplate
    {
        public string Context { get; set; }
        public string Collection { get; set; }
        public int? DefinitionId { get; set; }
        public string DownloadName { get; set; }
        public string Body { get; set; }
        public List<PrintingPreviewParameter> Parameters { get; set; }
    }

    public class PrintingPreviewParameter
    {
        public string Key { get; set; }
        public string Control { get; set; }
    }
}
