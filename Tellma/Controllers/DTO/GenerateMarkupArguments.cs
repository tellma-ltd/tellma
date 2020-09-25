using System.Collections.Generic;

namespace Tellma.Controllers.Dto
{
    public class GenerateMarkupArguments
    {
        public string Culture { get; set; }
    }

    public class GenerateMarkupByFilterArguments<TKey> : GenerateMarkupArguments
    {
        public string Filter { get; set; }
        public string OrderBy { get; set; }
        public int Top { get; set; }
        public int Skip { get; set; }
        public List<TKey> I { get; set; }
    }

    public class GenerateMarkupByIdArguments : GenerateMarkupArguments
    {
      //  public object Id { get; set; }
    }
}
