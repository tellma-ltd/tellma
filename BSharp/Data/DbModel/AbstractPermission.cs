using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data.DbModel
{
    public class AbstractPermission
    {
        public string ViewId { get; set; }

        public string Criteria { get; set; }

        public string Level { get; set; }
    }
}
