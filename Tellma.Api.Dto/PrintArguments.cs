using System.Collections.Generic;

namespace Tellma.Api.Dto
{
    public class PrintArguments
    {
        public string Culture { get; set; }
    }

    public class PrintEntitiesArguments<TKey> : PrintArguments
    {
        public string Filter { get; set; }
        public string OrderBy { get; set; }
        public int Top { get; set; }
        public int Skip { get; set; }
        public List<TKey> I { get; set; }
    }

    public class PrintEntityByIdArguments : PrintArguments
    {
    }
}
