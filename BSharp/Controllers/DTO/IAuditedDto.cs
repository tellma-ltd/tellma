using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    public interface IAuditedDto
    {
        DateTimeOffset? CreatedAt { get; set; }

        int? CreatedById { get; set; }

        DateTimeOffset? ModifiedAt { get; set; }

        int? ModifiedById { get; set; }
    }
}
