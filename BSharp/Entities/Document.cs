using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Entities
{
    [StrongEntity]
    public class DocumentForSave : EntityWithKey<int>
    {

    }

    public class Document : DocumentForSave
    {
        public string DocumentDefinitionId { get; set; }
    }
}
