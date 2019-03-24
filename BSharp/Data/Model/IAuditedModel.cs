using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data.Model
{
    public interface IAuditedModel
    {
        DateTimeOffset CreatedAt { get; set; }

        int CreatedById { get; set; }
        LocalUser CreatedBy { get; set; }

        DateTimeOffset ModifiedAt { get; set; }

        int ModifiedById { get; set; }
        LocalUser ModifiedBy { get; set; }
    }
}
