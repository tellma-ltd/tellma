using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.Identity
{
    public class DbUser
    {
        public int? Id { get; set; }
        public string ExternalId { get; set; }
        public string Email { get; set; }
    }
}
