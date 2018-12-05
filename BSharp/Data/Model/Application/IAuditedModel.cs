using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data.Model.Application
{
    public interface IAuditedModel
    {
        DateTimeOffset CreatedAt { get; set; }

        string CreatedBy { get; set; }

        DateTimeOffset ModifiedAt { get; set; }

        string ModifiedBy { get; set; }
    }
}
