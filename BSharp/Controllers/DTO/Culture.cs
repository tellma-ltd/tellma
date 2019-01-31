using BSharp.Controllers.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    [CollectionName("Cultures")]
    public class Culture : DtoKeyBase<string>
    {
        public string Name { get; set; }
        public string EnglishName { get; set; }
        public bool IsNeutralCulture { get; set; }
    }
}
