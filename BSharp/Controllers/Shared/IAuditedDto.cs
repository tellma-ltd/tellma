using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Shared
{
    public interface IAuditedDto
    {
        DateTimeOffset? CreatedAt { get; set; }

        string CreatedBy { get; set; }

        DateTimeOffset? ModifiedAt { get; set; }

        string ModifiedBy { get; set; }
    }
}
