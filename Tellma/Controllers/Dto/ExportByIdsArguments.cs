using System.Collections.Generic;

namespace Tellma.Controllers.Dto
{
    public class ExportByIdsArguments<TKey>
    {
        public List<TKey> I { get; set; }
    }
}
