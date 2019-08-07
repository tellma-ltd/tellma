using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Misc
{
    // TODO: Delete
    public class AbstractPermission
    {
        public string ViewId { get; set; }

        public string Action { get; set; }

        public string Criteria { get; set; }

        public string Mask { get; set; }
    }
}
