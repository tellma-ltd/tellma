using System.Collections.Generic;

namespace Tellma.Controllers.Dto
{
    public class CompaniesForClient
    {
        public bool IsAdmin { get; set; }
        public List<UserCompany> Companies { get; set; }
    }
}
