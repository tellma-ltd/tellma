using System.Collections.Generic;

namespace Tellma.Api.Dto
{
    public class PrintPreviewArguments
    {
        public string Culture { get; set; }
    }

    public class PrintEntitiesPreviewArguments<TKey> : PrintPreviewArguments
    {
        public string Filter { get; set; }
        public string OrderBy { get; set; }
        public int Top { get; set; }
        public int Skip { get; set; }
        public List<TKey> I { get; set; }
    }

    public class PrintEntityByIdPreviewArguments : PrintPreviewArguments
    {
    }
}
