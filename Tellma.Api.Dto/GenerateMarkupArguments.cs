using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tellma.Api.Dto
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
